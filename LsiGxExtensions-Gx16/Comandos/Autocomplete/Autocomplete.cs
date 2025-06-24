using ActiproSoftware.SyntaxEditor;
using Artech.Architecture.Common.Events;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Framework.Commands;
using Artech.FrameworkDE.Text;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using LSI.Packages.Extensiones.Comandos.Autocomplete.ObjectsInfoCache;
using LSI.Packages.Extensiones.Comandos.Autocomplete.ParmsInfo;
using LSI.Packages.Extensiones.Comandos.Autocomplete.PredictionBindings;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete
{
    // TODO: This class should be a singleton

    /// <summary>
    /// Code autocompletion
    /// </summary>
    class Autocomplete
    {

        /// <summary>
        /// Cache with object names and function names candidates to display on autocomplete
        /// </summary>
        static internal ObjectNamesCache NamesCache = new ObjectNamesCache();

        /// <summary>
        /// Predictions handler. It can be null if predictions are disabled. check Predictor.Ready
        /// </summary>
        static internal AutocompletePrediction Predictor;

        /// <summary>
        /// Supported object / part types for autocomplete
        /// </summary>
        static internal List<ObjectPartType> SupportedPartTypes;

        /// <summary>
        /// Execution time
        /// </summary>
        static private Stopwatch ExecutionTime = new Stopwatch();

		/// <summary>
		/// Time to generate popup options
		/// </summary>
		static private TimeSpan GetOptionsTime;

		/// <summary>
		/// Time to assign probabilites to options
		/// </summary>
		static private TimeSpan AssignProbabilitiesTime;

		/// <summary>
		/// Time to populate autocomplete popup
		/// </summary>
		static private TimeSpan PopulatePopupTime;

        /// <summary>
        /// Prefech open objects info needed by prediction (attributes types, object signatures), to make it faster
        /// </summary>
        static private OpenObjectsPrefech OpenObjectsPrefech;

        static public void Setup()
        {
            // When a object part is created on UI, register events to handle autocomplete
            UIServices.DocumentManager.DocumentPartEditorCreated += DocumentManager_DocumentPartEditorCreated;

            ObjectContextCache.RegisterEventHandlers();

            SupportedPartTypes = new List<ObjectPartType>();
			SupportedPartTypes.Add(ObjectPartType.ProcedureRules);
			SupportedPartTypes.Add(ObjectPartType.Procedure);
            SupportedPartTypes.Add(ObjectPartType.ProcedureWwPlus);
            SupportedPartTypes.Add(ObjectPartType.ProcedureConditions);

			SupportedPartTypes.Add(ObjectPartType.WorkPanelRules);
			SupportedPartTypes.Add(ObjectPartType.WorkPanelEvents);
            SupportedPartTypes.Add(ObjectPartType.WorkPanelConditions);

			SupportedPartTypes.Add(ObjectPartType.TransactionRules);
			SupportedPartTypes.Add(ObjectPartType.TransactionEvents);
            SupportedPartTypes.Add(ObjectPartType.TransactionEventsWwPlus);

            SupportedPartTypes.Add(ObjectPartType.WebPanelRules);
			SupportedPartTypes.Add(ObjectPartType.WebPanelEvents);
            SupportedPartTypes.Add(ObjectPartType.WebPanelEventsWwPlus);
            SupportedPartTypes.Add(ObjectPartType.WebPanelConditions);

			SupportedPartTypes.Add(ObjectPartType.SDPanelRules);
			SupportedPartTypes.Add(ObjectPartType.SDPanelEvents);
            SupportedPartTypes.Add(ObjectPartType.SDPanelConditions);
		}

        static public void SetupAutocompleteCurrentKb()
        {
            try
            {
                if (!LsiExtensionsConfiguration.Load().CustomAutocomplete)
                    return;

				NamesCache = new ObjectNamesCache();
                NamesCache.OnReady += (ObjectNamesCache cache) =>
                {
                    if(!cache.SetupFailed)
                        SetupPredictor();
                };
                NamesCache.SetupInThread();
            }
            catch (Exception e)
            {
                Log.ShowException(e);
                // Do no keep a cache that could be unusable
                NamesCache = new ObjectNamesCache();
            }
        }

        static public void DisableAutocompleteCurrentKb()
        {
            try
            {
                // Clear the kb object names cache
                NamesCache.Clear();
                DisposePredictor();
            }
            catch (Exception e)
            {
                Log.ShowException(e);
            }
        }

        static public void DisposePredictor()
        {
            Predictor?.Dispose();
            Predictor = null;

            OpenObjectsPrefech?.Dispose();
            OpenObjectsPrefech = null;
        }

        private static void RunAutocomplete(BaseSyntaxEditor syntaxEditor, bool forceShow, Keys keyData)
        {

            if (!NamesCache.Ready)
            {
                if (NamesCache.SetupFailed)
                    // Cache setup has failed. Maybe because it's a new kb (uncontrollable). Retry to setup cache
                    SetupAutocompleteCurrentKb();

                // Othersiwe, names cache is still loading values
                return;
            }

            try
            {
                ExecutionTime.Reset();
                ExecutionTime.Start();
				AssignProbabilitiesTime = GetOptionsTime = PopulatePopupTime = TimeSpan.Zero;
				TimeSpan before;

				SetupPredictor();

                // Get current context
                AutocompleteContext context = new AutocompleteContext(syntaxEditor, NamesCache, Predictor);

				if (context.Object.Type == ObjClass.SDPanel)
					// See "¯\_(ツ)_/¯" comments in ObjectContextCache for this (Spoiler: Variables cache cannot be implemented in SDPanels)
					ObjectContextCache.ClearVariablesCache();

                // Do not autocomplete inside comments or string constants, or after a native command ("CSHARP")
                if (!context.AutocompleteApplicable)
                    return;

                // The text before the caret on the current token
                string currentPrefix = context.LineParser.CurrentTokenPrefix;

                // Special case: Suggest sub names in DO '|'
                if (context.CursorInDoIdentifier)
                {
                    SubNames.SuggestSubIdentifiers(context, syntaxEditor);
                    return;
                }

                if (currentPrefix.Length == 0)
                {
                    // Nothing typed and not choosing a member (chosing a member => prefix = ".")
                    if (context.MemberList.Visible)
                        context.MemberList.Abort();
                    if (!forceShow)
                        return;
                }
                else if (keyData == Keys.Back && !context.MemberList.Visible)
                    // Back key pressed and members list is not currently visible: Do nothing
                    return;
				else
				{
					char firstChar = currentPrefix[0];

					// Are we tipyng ThemeClass:xx ?: Two cases: "ThemeClass:|" and "ThemeClass:Somethingtyped|"
					// "ThemeClass:" autocompletion is handled by Genexus, do nothing
					bool themeClass = ( (firstChar == ':' && context.LineParser.TokenBeforeCaret.ToLower() == KeywordGx.THEME_CLASS.ToLower() )
						|| (context.LineParser.TokenBeforeCaret == ":" && context.LineParser.GetCompletedTokenTextBackwards(1).ToLower() == KeywordGx.THEME_CLASS.ToLower()) );

					if (firstChar == '.' || firstChar == '&' || context.LineParser.TokenBeforeCaret == "." || themeClass)
					{
						// Displaying a member, variable of ThemeClass:
						// Genexus should already have displayed the autocomplete list
						if (!context.MemberList.Visible && forceShow)
						{
							// If it's not displayed, force to show it. There is no public member of Gx editor to do it, so emulate a Ctrl+Space type
							// TODO: This is not working...
							SendKeys.Send("^ ");
							return;
						}

						// No support for class prediction...
						if (themeClass)
							return;

						// Add probabilities and exit
						before = ExecutionTime.Elapsed;
						if (Predictor != null)
							// Use predictor
							Predictor.SelectMostProbableVariableOrMemberFromPrediction(context, context.MemberList);
						else if (firstChar == '&')
							// Check data types
							SelectMostProbableVariableAsParameter(context);
						AssignProbabilitiesTime = ExecutionTime.Elapsed.Subtract(before);

						return;
					}
					if (char.IsDigit(firstChar))
						// Number (do not autocomplete)
						return;
				}

				if ((keyData & Keys.Back) != 0 || (keyData & Keys.Left) != 0)
                {
                    // This will need more space for the autocomplete popup:
                    if (context.MemberList.Visible)
                        context.MemberList.Abort();
                }

				before = ExecutionTime.Elapsed;
				List<AutocompleteItem> options = AutocompleteItemsGeneration.GetOptions(context);
				GetOptionsTime = ExecutionTime.Elapsed.Subtract(before);

				if (options.Count == 0)
                    return;

				// Get magic prediction, if the function is enabled
				if (Predictor != null)
				{
					before = ExecutionTime.Elapsed;
					Predictor.AssingProbabilities(context, options);
					AssignProbabilitiesTime = ExecutionTime.Elapsed.Subtract(before);
				}

                // Execution time of this seems cannot be optmized
				before = ExecutionTime.Elapsed;
				context.MemberList.Clear();
				context.MemberList.AddRange(options.ToArray());
				context.MemberList.Show(syntaxEditor.Caret.Offset - currentPrefix.Length, currentPrefix.Length);
				// It seems selected item must to be set after show, or it will be lost
                context.MemberList.SelectedItem = AutocompleteItem.GetHighestPriorityItem(options);
				PopulatePopupTime = ExecutionTime.Elapsed.Subtract(before);

            }
            finally
            {
                ExecutionTime.Stop();
                DebugPrediction();
            }
        }

        /// <summary>
        /// Display debug information in the Output window
        /// </summary>
        private static void DebugPrediction()
        {
            if (!LsiExtensionsConfiguration.Load().DebugPredictionModel || Predictor == null)
                return;

            string predictionDebug = Predictor.GetNewPredictionDebugText();
            if (predictionDebug == null)
                return;

            using (Log log = new Log(false, false))
            {
                log.Output.AddLine(predictionDebug);
				if(GetOptionsTime != null)
					log.Output.AddLine("Autocomplete items generation: " + GetOptionsTime.Milliseconds + " ms");
				if(AssignProbabilitiesTime != null)
					log.Output.AddLine("Assign probabilities: " + AssignProbabilitiesTime.Milliseconds + " ms");
				if(PopulatePopupTime != null)
					log.Output.AddLine("Populate popup/assign most probable: " + PopulatePopupTime.Milliseconds + " ms");
				log.Output.AddLine("Total autocomplete time: " + ExecutionTime.ElapsedMilliseconds + " ms");
            }
        }

        private static void SetupPredictor()
        {
            try
            {
                if (Predictor == null &&
                    LsiExtensionsConfiguration.Load().PredictionModelType != LsiExtensionsConfiguration.PredictionModelTypes.DoNotUse)
                {
                    Predictor = new AutocompletePrediction(NamesCache);
                    OpenObjectsPrefech = new OpenObjectsPrefech(NamesCache);
                }
            }
            catch(Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        // TODO: This could be done too for attributes!
        // TODO: Try to use same code in AutocompletePrediction for this
        /// <summary>
        /// If we are on a object call, selects the most probable variable as parameter, without the prediction model
        /// </summary>
        /// <param name="context">Current context</param>
        private static void SelectMostProbableVariableAsParameter(AutocompleteContext context)
        {
            if (!context.MemberList.Visible)
                return;

            // Current typed prefix
            string prefix = context.LineParser.CurrentTokenPrefix.ToLower();
            if (prefix.Length == 0 && prefix[0] != '&')
                return;
            prefix = prefix.Substring(1);

            if (context.CallFinder.ParameterInfo == null)
                return;

            // Get variables
            VariablesPart  variables = context.Object.Parts.LsiGet<VariablesPart>();
            if (variables == null)
                return;

            // Add options, if needed, to create new variables
            new AddVariableAutocomplete(context).UpdateMembersList();

            double maxProbability = 0;
            IntelliPromptMemberListItem maxProbabilityItem = null;
            foreach (IntelliPromptMemberListItem item in context.MemberList)
            {
                // Do not compare with Text: It can contain extra text. The real text will be AutoCompletePreText
                //string txt = item.Text.ToLower();
                string txt = item.AutoCompletePreText.ToLower();
                if (txt.StartsWith(prefix))
                {
                    VariableNameInfo vNameInfo = ObjectContextCache.GetVariableFromCache(variables, item.AutoCompletePreText);
                    double probability = AutocompleteItem.GetParameterProbability(vNameInfo, context.CallFinder);
                    if (probability > maxProbability)
                    {
                        maxProbability = probability;
                        maxProbabilityItem = item;
                    }
                }
            }
            if (maxProbabilityItem != null)
                context.MemberList.SelectedItem = maxProbabilityItem;
        }

        static internal BaseSyntaxEditor GetEditorFromCommandData(CommandData commandData)
        {
            return GetEditorFromObject(commandData.Context);
        }

        static internal BaseSyntaxEditor GetEditorFromObject(object o)
        {
            Control control = o as Control;
            if (control == null)
                return null;
            return ControlUtils.GetControlsOfType<BaseSyntaxEditor>(control).FirstOrDefault();
        }

        /// <summary>
        /// Update "Autocomplete" command from current state
        /// </summary>
        /// <param name="data">Command to enable</param>
        /// <param name="status">Command status</param>
        /// <returns>True if the command state was set by this funcion</returns>
        static public bool QueryCommandAutocomplete(CommandData commandData, ref CommandStatus status)
        {
            try
            {
                status.State = CommandState.Enabled;
                KBObjectPart part = Entorno.CurrentEditingPart;
                if(part == null || !SupportedPartTypes.Contains(new ObjectPartType(part)))
                    status.State = CommandState.Disabled;

				if (!LsiExtensionsConfiguration.Load().CustomAutocomplete)
					status.State = CommandState.Disabled;

				return true;
            }
            catch
            {
                return false;
            }
            
        }

        /// <summary>
        /// Execute the autocomplete from the menu
        /// </summary>
        /// <param name="commandData">Command data</param>
        /// <returns>True if the command has been executed (?)</returns>
        static public bool Execute(CommandData commandData)
        {
            try
            {
                // Check if custom autocomplete is enabled
                if (!LsiExtensionsConfiguration.Load().CustomAutocomplete)
                    return false;

                BaseSyntaxEditor syntaxEditor = GetEditorFromCommandData(commandData);
                if (syntaxEditor == null)
                    return false;

                RunAutocomplete(syntaxEditor, true, 0);
                return true;
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                return false;
            }
        }

        #region *** EDITOR EVENTS ***

        private static void SyntaxEditor_KeyTyping(object sender, KeyTypingEventArgs e)
        {
            // Check if custom autocomplete is enabled
            if (!LsiExtensionsConfiguration.Load().CustomAutocomplete)
                return;

            SyntaxEditor syntaxEditor = sender as SyntaxEditor;
            if (syntaxEditor == null)
                return;

            SearchInAutocopleteTooltip(syntaxEditor, e);

            // TODO: Don't do this inside comments
            if ((e.KeyData & (Keys.Alt | Keys.Control)) != 0)
                return;

            BalancedChars.KeyTyping(syntaxEditor, e);

        }

        /// <summary>
        /// Makes Ctrl + Up / Ctrl + Down search in intellisense tooltip for typed text, in any position (not only prefix)
        /// </summary>
        /// <param name="syntaxEditor">Current text editor</param>
        /// <param name="e">Key typing event</param>
		private static void SearchInAutocopleteTooltip(SyntaxEditor syntaxEditor, KeyTypingEventArgs e)
		{
            try
            { 
                // If autocomplete tooltip is shown, something is typed, and
                // Ctrl + Up or Ctrl + Down is pressed, search text in autocompletion
                // items
                int increment;
                if (e.KeyData == (Keys.Control | Keys.Up))
                    increment = -1;
                else if (e.KeyData == (Keys.Control | Keys.Down))
                    increment = +1;
                else
                    return;

                var memberList = syntaxEditor.IntelliPrompt.MemberList;
                if (!memberList.Visible)
                    return;

                var lineParser = new LineParser(syntaxEditor);

                string prefix;
                if(AutocompleteContext.IsCursorInDoIdentifier(syntaxEditor, lineParser))
                {
                    // Curosr is at DO 'Name|'. It's a diferent case
                    BaseSyntaxEditor baseEditor = syntaxEditor as BaseSyntaxEditor;
                    if(baseEditor == null)
                        return;
                    prefix = SubNames.GetCurrentTokenPrefix(baseEditor);
                }
                else
				{
                    // Cursor is at normal code
                    prefix = lineParser.CurrentTokenPrefix;
                }
                    
                if (prefix.StartsWith("&") || prefix.StartsWith("'") || prefix.StartsWith("\""))
                {
                    // Variable / sub name
                    prefix = prefix.Substring(1);
                }
                
                if (string.IsNullOrEmpty(prefix))
                    return;

                // Search next item containing the typed prefix
                var selectedItem = memberList.SelectedItem;
                int startIndex = selectedItem == null ? 0 : memberList.IndexOf(selectedItem);
                int idx = startIndex;
                for (int i=1; i<memberList.Count; i++)
				{
                    idx += increment;
                    if (idx < 0)
                        idx = memberList.Count - 1;
                    else if (idx >= memberList.Count)
                        idx = 0;

					IntelliPromptMemberListItem item = memberList[idx];
                    if(item.AutoCompletePreText.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0)
					{
                        memberList.SelectedItem = item;
                        break;
					}
				}
                e.Cancel = true;
            }
            catch(Exception ex)
			{
                Log.ShowException(ex);
			}
        }

		/// <summary>
		/// Called when the autocomplete window is closed (after): Handle special options
		/// </summary>
		private static void SyntaxEditor_IntelliPromptMemberListClosed(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
			{
                if (e.Cancel)
                    return;
                SyntaxEditor syntaxEditor = sender as SyntaxEditor;
                if (syntaxEditor == null)
                    return;
                AutocompleteItem selectedItem = syntaxEditor.IntelliPrompt.MemberList.SelectedItem as AutocompleteItem;
                if (selectedItem == null)
                    return;

                if ((selectedItem.Tag as string) == AddVariableAutocomplete.CREATEVARIABLETAG)
                {
                    AutocompleteContext context = new AutocompleteContext(syntaxEditor, NamesCache, Predictor);
                    new AddVariableAutocomplete(context).AfterAutocompleteItemSelected(selectedItem);
                }
            }
            catch(Exception ex)
			{
                Log.ShowException(ex);
			}
        }

        private static void SyntaxEditor_SelectionChanged(object sender, SelectionEventArgs e)
        {
            try
            {
                if (!LsiExtensionsConfiguration.Load().CustomAutocomplete)
                    return;

                if (!NamesCache.Ready)
                    return;

                BaseSyntaxEditor syntaxEditor = sender as BaseSyntaxEditor;
                if (syntaxEditor == null)
                    return;

                new ParametersInfo(syntaxEditor, NamesCache).SelectionChanged(e);

			}
            catch(Exception ex) {
                Log.ShowException(ex);
            }
        }

        private static void SyntaxEditor_KeyTyped(object sender, KeyTypedEventArgs e)
        {
            try
            {
                // Check if custom autocomplete is enabled
                if (!LsiExtensionsConfiguration.Load().CustomAutocomplete)
                    return;

                // Ignore function shorcuts
                if ((e.KeyData & (Keys.Alt | Keys.Control)) != 0 && char.IsLetterOrDigit(e.KeyChar))
                    return;

                BaseSyntaxEditor syntaxEditor = sender as BaseSyntaxEditor;
                if (syntaxEditor == null)
                    return;

                new ParametersInfo(syntaxEditor, NamesCache).KeyTyped(e);

                if (e.Overwrite)
                    return;

                // Check autoclose characters
                BalancedChars.KeyTyped(syntaxEditor, e);

				// Just characters, numbers, backspace, dot, ampersand, quotes (for sub names)
				Keys keyWithoutModifiers = (e.KeyData & Keys.KeyCode);
				bool interestingKey = char.IsLetterOrDigit(e.KeyChar) || keyWithoutModifiers == Keys.Back || e.KeyChar == '.' || e.KeyChar == '&' ||
					e.KeyChar == '\'' || e.KeyChar == '\"';
				// Cursors right / left, only if autocomplete prompt is visible
				if (syntaxEditor.IntelliPrompt.MemberList.Visible && (keyWithoutModifiers == Keys.Left || keyWithoutModifiers == Keys.Right))
					interestingKey = true;

				if(interestingKey)
					RunAutocomplete(syntaxEditor, false, e.KeyData);
            }
            catch(Exception ex)
            {
                Log.ShowException(ex);
            }
        }

        #endregion

        #region *** KB EVENTS ***

        static public void AfterOpenKBEvent(object sender, EventArgs args)
        {
            SetupAutocompleteCurrentKb();
        }

        /// <summary>
        /// Called when a KB is closed. It clears the names cache
        /// </summary>
        static public void BeforeCloseKBEvent(object sender, EventArgs args)
        {
            DisableAutocompleteCurrentKb();
        }

        static public void OnBeforeSaveKBObject(object sender, KBObjectEventArgs args)
        {
            if (!NamesCache.Ready)
                return;

            // Object can be renamed, or description change: Remove and reinsert if needed
            NamesCache.AddOrUpdate(args.KBObject, loadLazyObjectInfo: true);
        }

        static public void OnAfterSaveKBObject(object sender, KBObjectEventArgs args)
		{
            NamesCache.FixTimestamp(args.KBObject);
        }

        static public void OnBeforeDeleteKBObject(object sender, KBObjectEventArgs args)
        {
            if (!NamesCache.Ready)
                return;

            NamesCache.RemoveObject(args.KBObject);
        }

        private static void DocumentManager_DocumentPartEditorCreated(object sender, DocumentPartEventArgs e)
        {
            try
            {
                // Check if the part is supported
                if (!SupportedPartTypes.Any(x => x.Match(e.DocumentPart.Part)))
                    return;

                UserControl editor = UIServices.Environment.ActiveView.ActiveView as UserControl;
                if (editor == null)
                    return;
                
                BaseSyntaxEditor syntaxEditor = ControlUtils.GetControlsOfType<BaseSyntaxEditor>(editor).FirstOrDefault();
                if (syntaxEditor == null)
                    return;

                syntaxEditor.KeyTyped += SyntaxEditor_KeyTyped;
                syntaxEditor.KeyTyping += SyntaxEditor_KeyTyping;
				syntaxEditor.IntelliPromptMemberListClosed += SyntaxEditor_IntelliPromptMemberListClosed;
                syntaxEditor.SelectionChanged += SyntaxEditor_SelectionChanged;
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

		#endregion

	}

}

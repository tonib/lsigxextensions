using ActiproSoftware.SyntaxEditor;
using Artech.Architecture.UI.Framework.Controls;
using Artech.Architecture.UI.Framework.Editors;
using Artech.Architecture.UI.Framework.Packages;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Framework.Commands;
using Artech.FrameworkDE.Controls;
using Artech.FrameworkDE.Text;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.UI.Common.Editor;
using Artech.Patterns.WorkWithDevices.Objects;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.UI;
using System;
using System.Linq;

namespace LSI.Packages.Extensiones.Comandos.Edit.AddVariable
{
    /// <summary>
    /// "Create a variable for the word the current word" command, improved
    /// </summary>
    class AddVariableFastKey
    {
        /// <summary>
        /// Execute the task
        /// </summary>
        static public bool AddVariable(CommandData data)
        {
            try
            {
                // If properties window has focus, return focus to text editor window and exit
                IPropertyInspector propertyInspector = UIServices.Property.GetPropertyInspector();
                if (propertyInspector == null)
                    return true;
                if (propertyInspector.Control.ContainsFocus && UIServices.Environment.ActiveView != null)
                {
                    UIServices.Environment.ActiveView.ActivateView();
                    return true;
                }

                Variable v = propertyInspector.SelectedObject as Variable;
                if (v != null)
                {
                    if (v.IsAutoDefined)
                    {
                        // If it's autodefined, define the variable
                        RunAddVariableCommand(data);
                        return true;
                    }

                    // Move focus to "Based on" property on the Property toolwindow, to edit the variable
                    propertyInspector.Control.Focus();
                    PropertyGridEx grid = ControlUtils.GetControlsOfType<PropertyGridEx>(propertyInspector.Control).FirstOrDefault();
                    if (grid != null)
                    {
                        grid.SelectedPropertyName = Properties.ATT.BasedOn;
                        grid.InternalGrid.Focus();
                    }
                }
                else
                {
                    // Try to create the variable from its name
                    if (!CheckCustomCreation(data, propertyInspector))
                        RunAddVariableCommand(data);
                }
            }
            catch(Exception ex)
            {
                Log.ShowException(ex);
            }

            return true;
        }

        static private void RunAddVariableCommand(CommandData data)
        {
            HideAutocompleteList(data);

            UIServices.CommandDispatcher.Dispatch(new CommandKey(UIPackageGuid.TextEditor,
                        Artech.Architecture.UI.Framework.Commands.CommandKeys.TextEditor.AddVariable.Name));
        }

        /// <summary>
        /// Run Genexus command to create a variable for word over the caret
        /// </summary>
        /*static public void FireAddVariableCommand()
		{
            UIServices.CommandDispatcher.Dispatch(new CommandKey(UIPackageGuid.TextEditor,
                        Artech.Architecture.UI.Framework.Commands.CommandKeys.TextEditor.AddVariable.Name));
        }*/

        static bool CheckCustomCreation(CommandData data, IPropertyInspector propertyInspector)
        {
            // Get text editor
            GxTextEditor editor = data.LsiGetCurrentTextEditor() as GxTextEditor;
            if (editor == null)
                return false;

            // Check token under the cursor is a variable
            TextToken t = editor.CurrentToken;
            if (t == null || t.Type != TokenType.String )
                return false;
            string tokenText = t.Value as string;
            if (string.IsNullOrEmpty(tokenText))
                return false;
            if (!tokenText.StartsWith("&"))
                return false;

            string varName = tokenText.Substring(1);
            BaseSyntaxEditor syntaxEditor = Autocomplete.Autocomplete.GetEditorFromCommandData(data);
            VariablesPart vPart = editor.GetVariablesPart();
            if (!CheckCustomCreation(syntaxEditor, vPart, varName, propertyInspector, data))
                return false;

            HideAutocompleteList(data);

            return true;
        }

        /// <summary>
        /// Perform a custom command to create a variable (with improvements)
        /// </summary>
        /// <param name="syntaxEditor">Current syntax editor</param>
        /// <param name="vPart">Object variables part</param>
        /// <param name="varName">Variable name to create, without ampersand</param>
        /// <param name="propertyInspector">Properties pane</param>
        /// <param name="data">If this execution was fired by the Add variable (fast key) command, the command. null otherwise</param>
        /// <returns>True if variable was finally created</returns>
        static public bool CheckCustomCreation(SyntaxEditor syntaxEditor, VariablesPart vPart, string varName, IPropertyInspector propertyInspector, CommandData data)
		{
            // Do not duplicate variables
            if (vPart == null || vPart.GetVariable(varName) != null)
                return false;

            // Try to create variable from current parameter call
            CustomVariablesCreator variableCreator = new CustomVariablesCreator(vPart);
            Variable v = variableCreator.CreateFromCurrentCallParameter(syntaxEditor, varName);

            if (v == null)
                // Try to create if from its name
                v = CreateVariableFromName(data, variableCreator, varName);

            if (v == null)
                return false;

            vPart.Add(v);

            // Needed for SDPanels. Otherwise variable do not appear in Variables part
            if (vPart.KBObject is SDPanel)
                vPart.KBObject.Parts.LsiUpdatePart(vPart);

            vPart.OnInvalidate();
            propertyInspector.SelectedObject = v;

            return true;
        }

        static private void HideAutocompleteList(CommandData data)
        {
            // If there was a Intelliprompt open, close it: After create the variable is no longer needed
            BaseSyntaxEditor syntaxEditor = Autocomplete.Autocomplete.GetEditorFromCommandData(data);
            if (syntaxEditor != null)
                syntaxEditor.IntelliPrompt.MemberList.Abort();
        }

        static private Variable CreateVariableFromName(CommandData data, CustomVariablesCreator variableCreator, string varName)
        {
            // Check if there is a selected text. In this case, variable will be created based on the selection string
            string varNameForCreation = varName;
            if (data != null)
            {
                string selectedText = data.LsiGetSelectedText();
                if (!string.IsNullOrEmpty(selectedText))
                    varNameForCreation = selectedText;
            }

            // Try to create variable from its name
            Variable v = variableCreator.CreateFromName(varNameForCreation);
            if (v == null)
                return null;

            // Change the variable name property does not work... (TODO: Try again, I think it works...)
            if (varNameForCreation != varName)
                v = v.LsiCloneRenamed(varName);

            return v;
        }

    }
}

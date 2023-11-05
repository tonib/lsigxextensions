using ActiproSoftware.SyntaxEditor;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Controls;
using Artech.Architecture.UI.Framework.Language;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.Variables;
using Artech.Genexus.Common.Types;
using Artech.Patterns.WorkWithDevices.Objects;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using LSI.Packages.Extensiones.Comandos.Edit.AddVariable;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete
{
	/// <summary>
	/// Handle options in autocomplete to add a new variable
	/// </summary>
	class AddVariableAutocomplete
	{
        /// <summary>
        /// Tag for "Add variable" autocomplete items
        /// </summary>
		public const string CREATEVARIABLETAG = "CREATEVARIABLE";

		AutocompleteContext Context;

        public AddVariableAutocomplete(AutocompleteContext context)
		{
			Context = context;
        }

        void RemoveAddVariableItems()
		{
			// Debug.WriteLine("RemoveAddVariableItem");
			List<IntelliPromptMemberListItem> createVarItems = Context.MemberList
                .Cast<IntelliPromptMemberListItem>()
                .Where(i => (i.Tag as string) == CREATEVARIABLETAG)
                .ToList();

            createVarItems.ForEach(item => Context.MemberList.Remove(item));
        }

        /// <summary>
        /// Updates options in autocomplete list to add new variables
        /// </summary>
        public void UpdateMembersList()
        {
            if (!LsiExtensionsConfiguration.Load().AutocompleteCreateVariables)
                return;
            if (!Autocomplete.NamesCache.Ready)
                return;

			// Current automplete window lenght (in chars)
			IEnumerable<IntelliPromptMemberListItem> currentOptions = Context.MemberList.Cast<IntelliPromptMemberListItem>();
            int currentWindowWidth = currentOptions.Max(item => item.Text.Length);

            RemoveAddVariableItems();

            // Get typed prefix
            string varNamePrefix = Context.LineParser.CurrentTokenPrefix;
            if (varNamePrefix.StartsWith("&"))
                varNamePrefix = varNamePrefix.Substring(1);
            if (varNamePrefix.Length == 0)
                return;

            // If there is any automplete option starting with typed text, there is nothing to do
            string prefixLowercase = varNamePrefix.ToLower();
            if (currentOptions.Any(i => i.Text.ToLower().StartsWith(prefixLowercase)))
                return;

            // Add options to create new variables
            List<IntelliPromptMemberListItem> itemsToAdd = new List<IntelliPromptMemberListItem>();
			ItemToCreateTypedVariable(itemsToAdd, varNamePrefix);
            ItemToCreateCurrentParameter(itemsToAdd);

            if (Context.Predictor != null)
			{
                // Get probable options from prediction
                CreateItems(itemsToAdd, Context.Predictor.GetProbableNamesForNewVariables(Context, prefixLowercase));
			}
            else
                // Get raw probable options
                ProbableObjectNames(itemsToAdd, varNamePrefix);

            if(itemsToAdd.Count > 0)
			{
                Context.MemberList.AddRange(itemsToAdd.ToArray());
                if (currentWindowWidth < itemsToAdd.Max(item => item.Text.Length))
                {
                    // Needed to make member list wider
                    Context.MemberList.Abort();
                    Context.MemberList.Show(Context.SyntaxEditor.Caret.Offset - varNamePrefix.Length, varNamePrefix.Length);
                }
            }
        }

		private void ProbableObjectNames(List<IntelliPromptMemberListItem> itemsToAdd, string varNamePrefix)
		{
			ObjectNamesCache namesCache = Autocomplete.NamesCache;

            string prefixLowercase = varNamePrefix.ToLower();

            // Get attributes, domains and sdt names
            const int NNAMESBYTYPE = 2;
            
            CreateItems(itemsToAdd, namesCache.GetAttributesByPrefix(prefixLowercase)
                .Take(NNAMESBYTYPE)
                .Cast<AttributeNameInfo>()
                .Select(attName => new VariableNameInfo(attName.Name, attName.DataType))
            );

            // TODO: Filtering by domains namespace would give better performance
            CreateItems(itemsToAdd, namesCache.GetObjectsByPrefix(prefixLowercase)
                .Where(n => n.Type == ChoiceInfo.ChoiceType.Domain)
                .Take(NNAMESBYTYPE)
                .Cast<KbObjectNameInfo>()
                .Select(domName => new VariableNameInfo(domName.Name, domName.DataType))
            );

            CreateItems(itemsToAdd, namesCache.GetSdtsByPrefix(prefixLowercase)
                .Take(NNAMESBYTYPE)
                .Select(sdtName => new VariableNameInfo(sdtName.Name, sdtName.DataType))
            );
        }

		void ItemToCreateCurrentParameter(List<IntelliPromptMemberListItem> itemsToAdd)
		{
			ParameterElement parmInfo = Context.CallFinder.ParameterInfo;
            if (parmInfo == null)
                return;

            //VariablesPart variables = Context.Object.Parts.LsiGet<VariablesPart>();
            //if (variables == null)
            //    return ;

            // This is what we should do, but it has very bad performance, as it opens the obejct
            //CustomVariablesCreator variableCreator = new CustomVariablesCreator(variables);
            //Variable v = variableCreator.CreateFromCurrentCallParameter(Context.SyntaxEditor, parmInfo.Name);
            //if (v == null)
            //    return;

            // TODO: Create item only if parm name starts with currently typed prefix
            // Aproximation:
            VariableNameInfo vName = new VariableNameInfo(parmInfo.Name, parmInfo.DataType);
            CreateItem(itemsToAdd, vName);
        }

        void ItemToCreateTypedVariable(List<IntelliPromptMemberListItem> itemsToAdd, string varName)
		{
			// Debug.WriteLine("varName: " + varName);
			if (!Variable.IsValidName(varName))
				return;

            VariableNameInfo vNameInfo = new VariableNameInfo(varName);
            CreateItem(itemsToAdd, vNameInfo);
		}

        private void CreateItems(List<IntelliPromptMemberListItem> itemsToAdd, IEnumerable<VariableNameInfo> vNamesInfo)
		{
            foreach (VariableNameInfo vNameInfo in vNamesInfo)
                CreateItem(itemsToAdd, vNameInfo);
		}

        private void CreateItem(List<IntelliPromptMemberListItem> itemsToAdd, VariableNameInfo vNameInfo)
		{
            // Do not repeat items to add
            if (Context.MemberList.Cast<IntelliPromptMemberListItem>().Union(itemsToAdd).Any(i => i.AutoCompletePreText == vNameInfo.Name))
                return;

			string description = $"<b>Add NEW variable {vNameInfo.Name}</b>";
			string itemText = $"{vNameInfo.Name} [NEW VARIABLE]";

            // Create create variable item (update existing item did not worked if delete key was pressed)
            // Add new item
            // Debug.WriteLine("Add / CurrentCreateVarItem.Text: " + itemText);
            // new AutocompleteItem(Context.MemberList, itemText, ChoiceInfo.ChoiceType.Variable, description);
            var item = new AutocompleteItem(Context.MemberList, vNameInfo, Context);
            item.Text = itemText;
            item.Description = description;
			item.AutoCompletePreText = vNameInfo.Name;
			item.Tag = CREATEVARIABLETAG;
            itemsToAdd.Add(item);
        }

		/// <summary>
		/// Handle event of autoocomplete item to add variable selected: Do the variable creation
		/// </summary>
		/// <param name="selectedItem">Selected item with create variable function</param>
		public void AfterAutocompleteItemSelected(AutocompleteItem selectedItem)
		{
            // Selected item was a "create variable" item. Do the variable creation

            VariablesPart variables = Context.Object.Parts.LsiGet<VariablesPart>();
            if (variables == null)
                return;

            IPropertyInspector propertyInspector = UIServices.Property.GetPropertyInspector();
            if (propertyInspector == null)
                return;

			string varName = selectedItem.AutoCompletePreText;
			if (!AddVariableFastKey.CheckCustomCreation(Context.SyntaxEditor, variables, varName, propertyInspector, null))
			{
                // We cannot fire the AddVariable genexus command here, as it will not work: If space key was typed, the cursor will not be over the variable word
                // More, i cannot call the add variable command execution, as it's a damn protected member... ¯\_(ツ)_/¯
                //AddVariableFastKey.FireAddVariableCommand();

                // Variable creations fails with standard variables. Just don't create them. ¯\_(ツ)_/¯
                if (VariableDefinition.IsStandard(varName) || VariableDefinition.IsNewStandard(variables, varName))
                    return;

                // Let's try this:
                var v = new Variable(varName, variables);
                DataType.SetDefault(v);
                variables.Add(v);

                // Needed for SDPanels. Otherwise variable do not appear in Variables part
                if (Context.Object is SDPanel)
                {
					// Starting from Gx16, if I don't update the source code, and variable name is typed fast, editor does weird things: It removes part, 
					// or all, typed var. name, and moves the cursor ¯\_(ツ)_/¯
					KBObjectPart fakePart = Context.Object.Parts.LsiConvertPart(Context.Part);
                    SourcePart sourcePart = fakePart as SourcePart;
                    if(sourcePart != null)
					{
                        sourcePart.Source = Context.SyntaxEditor.Document.Text;
                        Context.Object.Parts.LsiUpdatePart(fakePart);
                    }
                    variables.KBObject.Parts.LsiUpdatePart(variables);
                }

                variables.OnInvalidate();
                propertyInspector.SelectedObject = v;
            }
		}
    }
}

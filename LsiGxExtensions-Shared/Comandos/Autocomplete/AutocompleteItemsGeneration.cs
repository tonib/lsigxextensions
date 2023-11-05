using Artech.Architecture.UI.Framework.Language;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.Layout;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Comandos.Autocomplete.Commands;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using LSI.Packages.Extensiones.Comandos.Autocomplete.ObjectsInfoCache;
using System.Collections.Generic;
using System.Linq;
using Artech.Patterns.WorkWithDevices.Parts;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete
{

    /// <summary>
    /// Calculate autocomplete items for a given position on an editor
    /// </summary>
    class AutocompleteItemsGeneration
    {
        internal const int MAXENTRIES = 15;

        public static List<AutocompleteItem> GetOptions(AutocompleteContext context)
        {

            // Get cursor current word text
            string prefix = context.LineParser.CurrentTokenPrefix.ToLower();

            // If previous command is "PRINT", show printblocks names only
            if (context.LineParser.TokenBeforeCaret.ToLower() == KeywordGx.PRINT)
                return GetPrintBlocksNames(context, prefix);

            HashSet<ObjectNameInfo> names = new HashSet<ObjectNameInfo>();

            // Add non-attribute objects. Add MAXENTRIES items of each namespace 
            AddResults(context, Autocomplete.NamesCache.GetObjectsByPrefix(prefix, MAXENTRIES), names, false);

            // Current context can contain attributes?
            bool canContainAttributes = context.OutlineState.CanContainAttributes(context.Part);
            if(!canContainAttributes)
            {
                // Exception: Inside aggregate function calls (ex "sum()") there can be attributes 
                if (context.CallFinder.FunctionName != null &&
                    KeywordGx.AGGREGATE_FUNCTIONS.Contains(context.CallFinder.FunctionName.Name.ToLower()))
                    canContainAttributes = true;
            }

			// If current object is Transaction, only atts referenced in structure part are available. Get them (this will be null if is not a Trn)
			HashSet<string> transactionAttributeNames = TransactionAttributesNames(context, prefix);

			// Add names predicted by model (callables and attributes)
			if (Autocomplete.Predictor != null)
                Autocomplete.Predictor.AddProbableOptions(context, canContainAttributes, names, transactionAttributeNames);

            // Add attributes, if we don't use prediction model
            if (canContainAttributes && Autocomplete.Predictor == null)
                AddAttributes(context, prefix, names, transactionAttributeNames);

            // Convert to autocomplete items
            List<AutocompleteItem> result = new List<AutocompleteItem>(names.Count * 2);
            foreach (ObjectNameInfo name in names)
                result.Add(new AutocompleteItem(context.MemberList, name, context));

            // Add command entries
            result.AddRange(CommandsAutocomplete.GetCommands(context));

            // Add standard event names, if needed
            AddEventNames(context, prefix, result);

            // Add control names, if needed
            AddControlNames(context, prefix, result, transactionAttributeNames);

            // Sort results
            result = result.OrderBy(x => x.Text).ToList();
            return result;
        }

		/// <summary>
		/// Add attributes to autocomplete names list
		/// </summary>
		/// <param name="context">Curent autocomplete context</param>
		/// <param name="prefix">Current word prefix, lowercase</param>
		/// <param name="names">Autocomplete names list result</param>
		/// <param name="availableAttNames">List of attributes names, lowercase. If not null, and includeAttributes is true, only attributes
		/// included in this set will be added to the result (used in transactions to add only attributes in structure part)</param>
		private static void AddAttributes(AutocompleteContext context, string prefix, HashSet<ObjectNameInfo> names, HashSet<string> availableAttNames)
        {
            if (context.Object.Type != ObjClass.Transaction)
            {
                AddParmNameAsAttribute(context, prefix, names, availableAttNames);
                AddResults(context, Autocomplete.NamesCache.GetAttributesByPrefix(prefix), names, true);
            }
            else
            {
                // On transactions, purpose only attributes on transaction structure
				IEnumerable<ObjectNameInfo> attNames = availableAttNames.Select(name => Autocomplete.NamesCache.GetAttributeByExactName(name)).Where(name => name != null);
				AddResults(context, attNames, names, true);
            }
        }

		/// <summary>
		/// If current object is Transaction, get attribute names in structure that start with the current prefix
		/// </summary>
		/// <param name="context">Curent autocomplete context</param>
		/// <param name="prefix">Current word prefix, lowercase</param>
		/// <returns>Attribute names in structure that start with the given prefix. Null if current object is not a transaction</returns>
		private static HashSet<string> TransactionAttributesNames(AutocompleteContext context, string prefix)
		{
			Transaction t = context.Object as Transaction;
			if (t == null)
				return null;
			return new HashSet<string>(t.Structure.GetAttributes().Select(att => att.Name.ToLower()).Where(name => name.StartsWith(prefix)));
		}

        /// <summary>
        /// If we are located on a parameter call, propose the parameter attribute name
        /// </summary>
        /// <param name="context">Curent autocomplete prefix</param>
        /// <param name="prefix">Current word prefix, lowercase</param>
        /// <param name="names">Autocomplete names list result</param>
        internal static void AddParmNameAsAttribute(AutocompleteContext context, string prefix, HashSet<ObjectNameInfo> names, HashSet<string> availableAttName)
        {
            if (context.CallFinder.ParameterInfo == null)
                return;

            string currentParmName = context.CallFinder.ParameterInfo.Name.ToLower();
            if (!currentParmName.StartsWith(prefix))
                return;

            ObjectNameInfo name = Autocomplete.NamesCache.GetAttributeByExactName(currentParmName);
            if (name == null)
                return;

            if (availableAttName != null && !availableAttName.Contains(name.Name.ToLower()))
                return;

            names.Add(name);
        }

		/// <summary>
		/// Purpose form control names 
		/// </summary>
		/// <param name="context">Current context</param>
		/// <param name="prefix">Current typed word prefix, lowercase</param>
		/// <param name="result">Autocomplete names list result</param>
		/// <param name="availableAttNames">List of attributes names, lowercase. If not null, and includeAttributes is true, only attributes
		/// included in this set will be added to the result (used in transactions to add only attributes in structure part)</param>
		private static void AddControlNames(AutocompleteContext context, string prefix, List<AutocompleteItem> result, HashSet<string> availableAttNames)
        {
            if(!(context.Part is EventsPart) && !(context.Part is VirtualEventsPart) && !(context.Part is RulesPart) && 
				!(context.Part is VirtualRulesPart) )
                return;

            // Get the object control names
            ObjectControls controlNames = ObjectContextCache.GetObjectCache(context.Object).ControlNames;

            int nAdded = 0;
            foreach (ControlNameInfo controlName in controlNames.GetValuesByPrefix(prefix))
            {
                // Do not repeat names (Attributes controls will have the same name as control and attribute)
                if (result.Any(x => x.Text == controlName.Name))
                    continue;

				// Attributes in forms have the same name for control and for attribute itself. Att names number added to autocompelte is limited. It happens that
				// after added attributes, you add controls. So, you get in autocomplete popup att names that appear with control icon, and it's ugly.
				// We can control this in transactions. TODO: Check if it can be controlled in other form types
				if (availableAttNames != null && availableAttNames.Contains(controlName.Name.ToLower()))
					continue;

                result.Add(new AutocompleteItem(context.MemberList, controlName.Name, ChoiceInfo.ChoiceType.Control));
                nAdded++;
                if (nAdded >= MAXENTRIES)
                    return;
            }
            
        }

        /// <summary>
        /// Get event names appliable on the currrent cursor position
        /// </summary>
        /// <param name="context">Current context</param>
        /// <param name="prefix">Current typed word prefix, lowercase</param>
        /// <param name="result">List where to add new words</param>
        private static void AddEventNames(AutocompleteContext context, string prefix, List<AutocompleteItem> result)
        {
            
            // Add a fixed list
            if (!(context.Part is VirtualEventsPart) && !(context.Part is EventsPart))
                return;

            if (context.LineParser.TokenBeforeCaret.ToLower() != KeywordGx.EVENT)
                return;

            result.AddRange(context.Part.LsiGetStandardEventsNames()
                .Where(x => x.ToLower().StartsWith(prefix))
                .Select(x => new AutocompleteItem(x, 2))
            );
        }

        private static void AddResults(AutocompleteContext context, IEnumerable<ObjectNameInfo> names, 
            HashSet<ObjectNameInfo> result, bool limitResults)
        {
            int nAdded = 0;
            foreach (ObjectNameInfo name in names)
            {
                FunctionNameInfo fName = name as FunctionNameInfo;
                if (fName != null && !fName.CanBeUsed(context.ObjectPartType))
                    continue;

                if (result.Add(name) && limitResults)
                {
                    nAdded++;
                    if (nAdded >= MAXENTRIES)
                        break;
                }
            }
        }

        /// <summary>
        /// Get available print blocks on current object as autocomplete items
        /// </summary>
        /// <param name="context">Current context</param>
        /// <param name="prefix">Current text prefix</param>
        /// <returns>Autocomplete items for print blocks on current object</returns>
        private static List<AutocompleteItem> GetPrintBlocksNames(AutocompleteContext context, string prefix)
        {
            List<AutocompleteItem> result = new List<AutocompleteItem>();

            LayoutPart layout = context.Object.Parts.Get<LayoutPart>();
            if (layout == null)
                return result;

            foreach (IReportBand printBlock in layout.Layout.ReportBands
                .Where(x => x.Name.ToLower().StartsWith(prefix))
                .OrderBy(x => x.Name))
                result.Add(new AutocompleteItem(context.MemberList, printBlock));

            return result;
        }

    }
}

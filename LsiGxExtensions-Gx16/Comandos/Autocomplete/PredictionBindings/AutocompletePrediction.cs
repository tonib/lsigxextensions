using ActiproSoftware.SyntaxEditor;
using Artech.Architecture.UI.Framework.Language;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.PredictionGeneration;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.PredictionBindings
{
    /// <summary>
    /// Binding between the prediction model and the automplete functions.
    /// </summary>
    public class AutocompletePrediction : IDisposable
    {
        /// <summary>
        /// Predictions generator, with cache for last prediction
        /// </summary>
        internal PredictionCache Predictor;

        private ObjectNamesCache NamesCache;

        /// <summary>
        /// Last debug prediction time
        /// </summary>
        private DateTime LastPredictionDebugTimestap;

        public AutocompletePrediction(ObjectNamesCache namesCache)
        {
            NamesCache = namesCache;
            Predictor = new PredictionCache(NamesCache);
        }

        /// <summary>
        /// Calculate prediction for the next token on the current editing object part
        /// </summary>
        /// <param name="context">Current autocomplete context</param>
        /// <returns>Next token prediction. null if it cannot be calculated</returns>
        private PredictionResult GetPrediction(AutocompleteContext context)
        {
            try
            {
                PredictionResult prediction = Predictor.GetPredictionFromCurrentPart(context);
                return prediction;
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                return null;
            }
        }

        public void AssingProbabilities(AutocompleteContext context, List<AutocompleteItem> options)
        {
            PredictionResult p = GetPrediction(context);
            if (p == null)
                return;
            
            bool debug = LsiExtensionsConfiguration.Load().DebugPredictionModel;
            string prefix = context.LineParser.CurrentTokenPrefix.ToLower();
            foreach( AutocompleteItem item in options )
            {
                if (item.Text.ToLower() == prefix)
                    // Explanation: Predictor will fail. Ex.: There are attributes called "FacFec" and "FacFecCre"
                    // If the user types "FacFec", the predictor can predict "FacFecCre". If this happens, ignore
                    // prediction and set the typed name as the most probable
                    item.Priority = double.MaxValue;
                else
                {
                    //p.SetPriority(item, cfg);
                    TokenizedAutocompleteItem tokenItem = new TokenizedAutocompleteItem(item, p.KbInfo, context.Object);
                    item.Priority = p.KbInfo.DataInfo.GetTokenProbability(tokenItem.Token, p);
                    if(debug)
                        tokenItem.AddDebugInfo(item, item.Priority, p);
                }
            }
            
        }

		void SelectMostProbableItem(AutocompleteContext context, IntelliPromptMemberList memberList, string prefix, PredictionResult prediction,
            Func<IntelliPromptMemberListItem, string, TokenizedAutocompleteItem> tokenGenerator)
		{
            bool debug = LsiExtensionsConfiguration.Load().DebugPredictionModel;

            double maxProbability = 0;
            IntelliPromptMemberListItem maxProbabilityItem = null;

            // If there is any Intelliprompt item that starts with the typed text, search by prefix
            // If not, user does not know what is typing: Search items that contain typed text
            // Do not compare with Text: It can contain extra text. The real text will be AutoCompletePreText
            var membersToCheck = memberList
                .Cast<IntelliPromptMemberListItem>()
                .Where(item => item.AutoCompletePreText.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
            if(!membersToCheck.Any())
			{
                membersToCheck = memberList
                    .Cast<IntelliPromptMemberListItem>()
                    .Where(item => item.AutoCompletePreText.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            foreach (IntelliPromptMemberListItem item in membersToCheck)
            {
                
                // string itemNameLowercase = item.Text.ToLower();
                string itemNameLowercase = item.AutoCompletePreText.ToLower();
                double probability;
                if (itemNameLowercase == prefix)
                {
                    // Explanation: Predictor will fail. Ex.: There are attributes called "FacFec" and "FacFecCre"
                    // If the user types "FacFec", the predictor can predict "FacFecCre". If this happens, ignore
                    // prediction and set the typed name as the most probable
                    probability = double.MaxValue;
                }
                else
                {
                    TokenizedAutocompleteItem tokenizedItem = tokenGenerator(item, itemNameLowercase);
                    probability = prediction.KbInfo.DataInfo.GetTokenProbability(tokenizedItem.Token, prediction);
                    if (debug)
                        tokenizedItem.AddDebugInfo(item, probability, prediction);
                }

                if (probability > maxProbability)
                {
                    maxProbability = probability;
                    maxProbabilityItem = item;
                }
            }
            if (maxProbabilityItem != null)
                memberList.SelectedItem = maxProbabilityItem;
        }

        void SelectMostProbableVariable(AutocompleteContext context, IntelliPromptMemberList memberList, string prefix, PredictionResult prediction)
		{
            VariablesPart variables = UIServices.Environment.ActiveDocument.Object.Parts.LsiGet<VariablesPart>();
            if (variables == null)
                return;

            // Add options, if needed, to create new variables
            new AddVariableAutocomplete(context).UpdateMembersList();

            // Generate token from autocomplete list item
            Func<IntelliPromptMemberListItem, string, TokenizedAutocompleteItem> tokenGenerator = (IntelliPromptMemberListItem item, string itemNameLowercase) =>
            {
                AutocompleteItem autocompleteItem = item as AutocompleteItem;
                VariableNameInfo vNameInfo = autocompleteItem?.Info as VariableNameInfo;
                if (vNameInfo != null)
                {
                    // This is used when the item is a "add variable" item. In this case, variable does not exists yet, and .Info contains the variable definition 
                    // to create
                    return new TokenizedAutocompleteItem(vNameInfo, prediction.KbInfo, context.Object);
                }
                else
                    // Existing variable
                    return new TokenizedAutocompleteItem(variables, item.Text, itemNameLowercase, prediction.KbInfo, context.Object);
            };

			SelectMostProbableItem(context, memberList, prefix, prediction, tokenGenerator);
        }

        void SelectMostProbableMember(AutocompleteContext context, IntelliPromptMemberList memberList, string prefix, PredictionResult prediction)
        {
            // Member: Get base reference. There is two cases: 1) "base.|" 2) "base.somethingtyped|"
            IToken baseToken = null;
            if (context.LineParser.TokenBeforeCaret == "." && context.LineParser.CompletedTokensCount >= 2)
                // Case 2:
                baseToken = context.LineParser.GetCompletedTokenBackwards(1);
            else if (context.LineParser.CompletedTokensCount >= 1)
                // Case 1:
                baseToken = context.LineParser.GetCompletedTokenBackwards(0);

            TextStream stream = context.SyntaxEditor.Document.GetTextStream(0);
            CodeTokenizer tokenizer = new CodeTokenizer(this.Predictor.Predictor.KbInfo, context.Part);

            // Generate token from autocomplete list item
            Func<IntelliPromptMemberListItem, string, TokenizedAutocompleteItem> tokenGenerator = (IntelliPromptMemberListItem item, string itemNameLowercase) =>
                new TokenizedAutocompleteItem(item.Text, itemNameLowercase, stream, tokenizer, baseToken);

            SelectMostProbableItem(context, memberList, prefix, prediction, tokenGenerator);
        }

        public void SelectMostProbableVariableOrMemberFromPrediction(AutocompleteContext context, IntelliPromptMemberList memberList)
        {
            if (!memberList.Visible)
                return;

            PredictionResult prediction = GetPrediction(context);
            if (prediction == null)
                return;

            // Current typed prefix
            string prefix = context.LineParser.CurrentTokenPrefix.ToLower();
            bool isVariable = false;
            if (prefix.Length > 0)
            {
                if (prefix[0] == '&')
                {
                    isVariable = true;
                    prefix = prefix.Substring(1);
                }
                else if (prefix[0] == '.')
					// This always will leave prefix empty ???
                    prefix = prefix.Substring(1);
            }

            if (isVariable)
                SelectMostProbableVariable(context, memberList, prefix, prediction);
            else
                SelectMostProbableMember(context, memberList, prefix, prediction);
        }


		/// <summary>
		/// Add probable objects/attributes based on model name prediction
		/// </summary>
		/// <param name="context">Current autocomplete context</param>
		/// <param name="includeAttributes">True if attributes should be included in result</param>
		/// <param name="result">Set where to add new names</param>
		/// <param name="availableAttNames">List of available attributes names in current object, lowercase. 
        /// If not null, and includeAttributes is true, only attributes
		/// included in this set will be added to the result (used in transactions to add only attributes in structure part)</param>
		public void AddProbableOptions(AutocompleteContext context, bool includeAttributes, HashSet<ObjectNameInfo> result, HashSet<string> availableAttNames)
        {
            // Get prediction
            PredictionResult prediction = GetPrediction(context);
            if (prediction == null)
                return;

			// Get predictions for each hash name column
			List<string> hashColumnNames = prediction.KbInfo.DataInfo.TextHashColumnNames;
			if (hashColumnNames.Count == 0)
				return;
			ColumnPrediction[] hashNameColumns = hashColumnNames.Select(colName => prediction.Columns[colName]).ToArray();

            // Get KB names classified by hash
            HashedNames hashedNames = prediction.KbInfo.ObjectNames.HashedNames;
            if (hashedNames == null)
                return;

            // Check 4 hashes for each column. There are 3 columns, so we will check 4^3 = 64 hashes combinations
            HashesCombinator hashesCombinator = new HashesCombinator(prediction, hashNameColumns, 4);

			// Get probable attributes
            if(includeAttributes)
			{
                string prefixLowercase = context.LineParser.CurrentTokenPrefix.ToLower();
                HashSet<ObjectNameInfo> probableAtts = new ProbableAttributes(context, prediction, hashesCombinator, NamesCache, prefixLowercase, availableAttNames)
                    .GetProbableAttributes(true);
                result.LsiAddRange(probableAtts);
            }

            // Get non attribute probable objects
            foreach (ObjectNameInfo name in EnumerateProbableNonAttNames(hashesCombinator, prediction, context.LineParser.CurrentTokenPrefix))
            {                    
                if (result.Add(name) && result.Count >= AutocompleteItemsGeneration.MAXENTRIES)
                    return;
            }
            
        }

        IEnumerable<ObjectNameInfo> EnumerateProbableNonAttNames(HashesCombinator hashesCombinator, PredictionResult prediction, string prefix)
		{
            HashedNames hashedNames = prediction.KbInfo.ObjectNames.HashedNames;
            if (hashedNames == null)
                yield break;

            foreach (int[] hashes in hashesCombinator)
            {
                foreach (ObjectNameInfo name in hashedNames.GetByHashAndPrefix(hashes, prefix)
                    .Where(n => n.Type != ChoiceInfo.ChoiceType.Attribute))
                {
                    yield return name;
                }
            }
        }

        public List<VariableNameInfo> GetProbableNamesForNewVariables(AutocompleteContext context, string prefixLowercase)
        {
            // TODO: Duplicated code in GetProbableNamesForNewVariables / AddProbableOptions
            var result = new List<VariableNameInfo>();
            PredictionResult prediction = GetPrediction(context);
            if (prediction == null)
                return result;

            // Get predictions for each hash name column
            List<string> hashColumnNames = prediction.KbInfo.DataInfo.TextHashColumnNames;
            if (hashColumnNames.Count == 0)
                return result;
            ColumnPrediction[] hashNameColumns = hashColumnNames.Select(colName => prediction.Columns[colName]).ToArray();

            // Check 4 hashes for each column. There are 3 columns, so we will check 4^3 = 64 hashes combinations
            HashesCombinator hashesCombinator = new HashesCombinator(prediction, hashNameColumns, 4);

			// Get probable attribute names
			List<TypedNameInfo> names = new ProbableAttributes(context, prediction, hashesCombinator, NamesCache, prefixLowercase, null)
                .GetProbableAttributes(false)
                .Cast<TypedNameInfo>()
                .ToList();
            // Get domain names. TODO: Filter by domain namespace should increase performance
            names.AddRange(
                NamesCache.GetObjectsByPrefix(prefixLowercase)
                .Where(n => n.Type == ChoiceInfo.ChoiceType.Domain)
                .Take(AutocompleteItemsGeneration.MAXENTRIES)
                .Cast<TypedNameInfo>()
            );
            // Get SDT names
            names.AddRange(NamesCache.GetSdtsByPrefix(prefixLowercase).Take(AutocompleteItemsGeneration.MAXENTRIES).Cast<TypedNameInfo>());

            // Gest most probable options
            return names
                .Select((name) =>
                {
                    // From picked names, get its probability
                    VariableNameInfo vName = new VariableNameInfo(name.Name, name.DataType);
                    TokenizedAutocompleteItem tokenizedItem = new TokenizedAutocompleteItem(vName, prediction.KbInfo, context.Object);
				    double probability = prediction.KbInfo.DataInfo.GetTokenProbability(tokenizedItem.Token, prediction);
                    return new KeyValuePair<VariableNameInfo, double>(vName, probability);
                })
                .OrderByDescending(kv => kv.Value)
                .Take(3)
                .Select(kv => kv.Key)
                .ToList();
		}

        /// <summary>
        /// Returns prediction debug text for last prediction since last call
        /// </summary>
        /// <returns>Text with prediction debug info. null if there is no new prediction since last call</returns>
        public string GetNewPredictionDebugText()
        {
            if (Predictor.LastPredictionTimestamp == LastPredictionDebugTimestap)
                return null;
            LastPredictionDebugTimestap = Predictor.LastPredictionTimestamp;

            if (Predictor.LastPrediction == null)
                return null;

            /*return "Input JSON: " + Predictor.LastPrediction.Input.ToJson() + Environment.NewLine  +
                "Output JSON: " + Predictor.LastPrediction.OutputJson + Environment.NewLine + 
                "PREDICTION:" + Predictor.LastPrediction.ToString();*/
			return "Raw input: " + Predictor.LastPrediction.Input.ToJson() + Environment.NewLine +
				"Input tokens: " + Predictor.LastPrediction.Input.Tokens.ToString(Predictor.Predictor.KbInfo.DataInfo) + 
                Environment.NewLine +
				"Input context: " + Predictor.LastPrediction.Input.Context + Environment.NewLine +
				"Prediction: " + Environment.NewLine + Predictor.LastPrediction.ToString() + 
                Environment.NewLine + Environment.NewLine;
		}

        public void Dispose()
        {
            try
            {
                if (Predictor == null)
                    return;
                Predictor.Dispose();
            }
            catch { }
            finally
            {
                Predictor = null;
            }
        }


    }
}

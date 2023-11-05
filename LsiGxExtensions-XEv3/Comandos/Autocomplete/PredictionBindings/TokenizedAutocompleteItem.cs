using ActiproSoftware.SyntaxEditor;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Language;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.PredictionGeneration;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using System.Collections.Generic;
using System.Linq;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition;
using LSI.Packages.Extensiones.Comandos.Autocomplete.ObjectsInfoCache;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.PredictionBindings
{
    /// <summary>
    /// Tokenized version of an autocomplete item
    /// </summary>
    public class TokenizedAutocompleteItem
    {
        /// <summary>
        /// Token info for the autocomplete item
        /// </summary>
        public TokenInfo Token;

        /// <summary>
        /// Tokenize a KbObject or command word
        /// </summary>
        /// <param name="item">The word autocomplete item</param>
        /// <param name="kbInfo">Language info</param>
        public TokenizedAutocompleteItem(AutocompleteItem item, KbPredictorInfo kbInfo, KBObject o)
        {
            // If the item text has more than one word, check only the first
            string tokenText = item.Text.Trim();
            if (tokenText.Contains(' '))
            {
                string[] words = tokenText.Split(new char[] { ' ' });
                if (words.Length > 0)
                    tokenText = words[0];
            }
            else if (tokenText == KeywordGx.NEWSDTOPERATOR)
                // Special case "new()"
                tokenText = "new";

            WordTypeKey wordType = ChoiceToWordType(item.Type);
            Token = new TokenInfo(wordType, tokenText, string.Empty, item.Info, kbInfo, o, null, -1);
        }

        /// <summary>
        /// Tokenize a variable name
        /// </summary>
        /// <param name="variables">Variables part</param>
        /// <param name="vNameOriginalcase">Variable name, with original case</param>
        /// <param name="vNameLowercase">Variable name, lowercase</param>
        /// <param name="kbInfo">Language info</param>
        /// <param name="o">KbObject token owner</param>
        public TokenizedAutocompleteItem(VariablesPart variables, string vNameOriginalcase, 
            string vNameLowercase, KbPredictorInfo kbInfo, KBObject o)
        {
            VariableNameInfo nameInfo = ObjectContextCache.GetVariableFromCache(variables, vNameLowercase);
            Token = new TokenInfo(WordTypeKey.Variable, vNameOriginalcase, string.Empty, nameInfo, kbInfo, o, null, -1);
        }

        /// <summary>
        /// Tokenize a variable name
        /// </summary>
        /// <param name="nameInfo">Variable name info</param>
        /// <param name="kbInfo">Language info</param>
        /// <param name="o">KbObject token owner</param>
        public TokenizedAutocompleteItem(VariableNameInfo nameInfo, KbPredictorInfo kbInfo, KBObject o)
        {
            Token = new TokenInfo(WordTypeKey.Variable, nameInfo.Name, string.Empty, nameInfo, kbInfo, o, null, -1);
        }

        /// <summary>
        /// Tokenize a member name
        /// </summary>
        /// <param name="textOriginalCase">Member name, original case</param>
        /// <param name="textLowercase">Member name, lowercase</param>
        /// <param name="codeTokenizer">Code tokenizer for current object part</param>
        /// <param name="baseToken">Base token for member (ex. "baseToken.textOriginalCase"). Null if there is no base token</param>
        public TokenizedAutocompleteItem(string textOriginalCase, string textLowercase, TextStream stream, CodeTokenizer codeTokenizer, IToken baseToken)
        {
            ObjectNameInfo attributeInfo = null;
            WordTypeKey wordType = codeTokenizer.GetMemberType(textLowercase, ref attributeInfo, stream, baseToken);
            Token = new TokenInfo(wordType, textOriginalCase, string.Empty, attributeInfo, codeTokenizer.KbInfo, codeTokenizer.CurrentPart.KBObject, null, -1);
        }

        public void AddDebugInfo(IntelliPromptMemberListItem item, double probability, PredictionResult prediction)
        {
            if (string.IsNullOrEmpty(item.Description))
                item.Description = string.Empty;
            else if (item.Description.EndsWith("%") || item.Description.EndsWith("]"))
                // Already added
                return;
            else
                item.Description += "<br/>";

			string probabilityFormat = "{0:0.000000}";
			item.Description += "<b>" + string.Format(probabilityFormat, probability * 100.0) + " %</b>";

			DataInfo dataInfo = prediction.KbInfo.DataInfo;

			List<string> dbgOutputColumnsNonName = new List<string>(dataInfo.OutputColumns.Count);
			List<string> dbgOutputColumnsName = new List<string>(dataInfo.TextHashColumnNames.Count);
			double nonNameProb = 1.0, nameProb = 1.0;
			foreach (string columnName in dataInfo.OutputColumns)
			{
				ColumnInfo column = dataInfo.GetColumnDefinition(columnName);
				int labelIdx = column.GetIntValue(Token, null, false);
				double labelProb = labelIdx >= 0 ? prediction.GetColumn(columnName).Probabilities[labelIdx] : 0.0;
				string debugText = string.Format("{0}: \"{1}\", {2} %",
					columnName,
					column.GetCsvDebugValue(Token, null),
					string.Format(probabilityFormat, labelProb * 100.0)
					);
				if (dataInfo.TextHashColumnNames.Contains(column.Name))
				{
					dbgOutputColumnsName.Add(debugText);
					nameProb *= labelProb;
				}
				else
				{
					dbgOutputColumnsNonName.Add(debugText);
					nonNameProb *= labelProb;
				}
			}
			item.Description += "<br/><br/>Non name:<br/>" + string.Join(" / ", dbgOutputColumnsNonName.ToArray()) +
				"<br/>Prob. non name: " + string.Format(probabilityFormat, nonNameProb * 100.0) +
				"% <br/><br/>Name: " + string.Join(" / ", dbgOutputColumnsName.ToArray()) +
				"<br/>Prob. name: " + string.Format(probabilityFormat, nameProb * 100.0) + " %";
		}

        static private WordTypeKey ChoiceToWordType(ChoiceInfo.ChoiceType choice)
        {
			// TODO: Check if there is a Gx function for this conversion
			// Meanwhile TokenizedAutocompleteItem.ChoiceToWordType and ObjectNameInfo constructors should be paired

			switch (choice)
            {
                case ChoiceInfo.ChoiceType.None:
				case ChoiceInfo.ChoiceType.NameSpace:
					return WordTypeKey.Keyword;

                case ChoiceInfo.ChoiceType.Attribute:
                    return WordTypeKey.Attribute;

                case ChoiceInfo.ChoiceType.Variable:
                    return WordTypeKey.Variable;

                case ChoiceInfo.ChoiceType.Function:
                    return WordTypeKey.Function;

                case ChoiceInfo.ChoiceType.Transaction:
                case ChoiceInfo.ChoiceType.WebPanel:
                case ChoiceInfo.ChoiceType.SDT:
                case ChoiceInfo.ChoiceType.Procedure:
                case ChoiceInfo.ChoiceType.DataSelector:
                case ChoiceInfo.ChoiceType.WorkPanel:
                case ChoiceInfo.ChoiceType.Domain:
                case ChoiceInfo.ChoiceType.DataProvider:
                case ChoiceInfo.ChoiceType.ExternalObject:
                case ChoiceInfo.ChoiceType.WorkWithDevices:
                case ChoiceInfo.ChoiceType.Dashboard:
                case ChoiceInfo.ChoiceType.SDPanel:
                case ChoiceInfo.ChoiceType.OfflineDatabase:
                case ChoiceInfo.ChoiceType.Image:
                case ChoiceInfo.ChoiceType.ThemeClass:
				case ChoiceInfo.ChoiceType.Module:
					return WordTypeKey.KbObject;

                case ChoiceInfo.ChoiceType.Control:
                    return WordTypeKey.Control;

                default:
                    return WordTypeKey.UnknownIdentifier;
            }
        }
    }
}

using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using LSI.Packages.Extensiones.Comandos.Autocomplete.Commands;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using LSI.Packages.Extensiones.Comandos.Autocomplete.ObjectsInfoCache;
using System;
using System.Linq;
using System.Collections.Generic;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor
{
	/// <summary>
	/// Information about a code word
	/// </summary>
	public class TokenInfo
    {

        /// <summary>
        /// Word type
        /// </summary>
        public WordTypeKey WordType;

        /// <summary>
        /// GX text editor token type. ONLY FOR DEBUG, it can be null
        /// </summary>
        public string GxTokenType;

		/// <summary>
		/// Token raw text, lowercase. It can be null if token has not an identfier (variable, attribute, ...).
		/// Variable names start with ampersand.
		/// </summary>
		public string TextLowercase;

        /// <summary>
        /// Token text, with original case. It can be null if token has not an identfier (variable, attribute, ...). 
		/// Variable names start with ampersand.
        /// </summary>
        public string Text;

        /// <summary>
        /// Token data type (real type for attr/variables/domains, returned data type for functions)
        /// </summary>
        public DataTypeInfo DataType = DataTypeInfo.NoType;

        /// <summary>
        /// Kb object type. Empty if is not a KbObject
        /// </summary>
        public Guid KbObjectType;

        /// <summary>
        /// The UI control info. related to this token. null if it has no related control
        /// </summary>
        private ControlNameInfo ControlName;

		/// <summary>
		/// Token position in code document
		/// </summary>
		public int TokenOffset;

		/// <summary>
		/// Token context. Can be null
		/// </summary>
		public TokenContext Context;

        /// <summary>
        /// Info about the token name. It can be null
        /// </summary>
        public ObjectNameInfo NameInfo;

        /// <summary>
        /// Create a token
        /// </summary>
        /// <param name="wordType">Prediction token type</param>
        /// <param name="tokenText">Token text, with original case on source. If it's a variable name, it must start with ampersand. If it has not, 
        /// it will added here</param>
        /// <param name="gxTokenType">Genexus token type</param>
        /// <param name="nameInfo">Name info, if word is an identifier (variable, attribute,...). null otherwise</param>
        /// <param name="kbInfo">Kb names info</param>
        /// <param name="kbObject">kbObject token owner</param>
        /// <param name="context">Token context. Can be null</param>
        /// <param name="tokenOffset">Token position in code document</param>
        public TokenInfo(WordTypeKey wordType, string tokenText, string gxTokenType, 
            ObjectNameInfo nameInfo, KbPredictorInfo kbInfo, KBObject kbObject, TokenContext context, int tokenOffset)
        {

			// Don't break CSV format:
			if (tokenText == "\"")
                tokenText = "'";

			// Variable names MUST start with &
			if (wordType == WordTypeKey.Variable && !tokenText.StartsWith("&"))
				tokenText = "&" + tokenText;

			WordType = wordType;
            Text = tokenText;
            if(tokenText != null)
                TextLowercase = tokenText.ToLower();
            GxTokenType = gxTokenType;
            TokenOffset = tokenOffset;
            NameInfo = nameInfo;

            // Store KbObject type
            if (wordType == WordTypeKey.KbObject && nameInfo != null)
            {
				KbObjectNameInfo koBObjectName = nameInfo as KbObjectNameInfo;
#if DEBUG
				if (koBObjectName == null)
					throw new Exception("wordType == WordTypeKey.KbObject but word is not a KbObjectNameInfo");
#endif
                KbObjectType = koBObjectName == null ? ObjClass.Procedure : koBObjectName.ObjectKey.Type;
            }

            // Get data type
            StoreDataType(nameInfo, kbInfo);

            // Get control info
            StoreControl(kbObject);

			Context = context;
		}

        private void StoreDataType(ObjectNameInfo nameInfo, KbPredictorInfo kbInfo)
        {
			/*if (kbInfo.DataInfo.Compatibility.SetDataTypeForConstants)
			{
				// Don't do this in new models: There are places where only can be a constant. If constants have type
				// model gets confused (ex. Event | purposes Chr() sometimes)
				if (WordType == WordTypeKey.StringConstant)
				{
					DataType = new DataTypeInfo(eDBType.CHARACTER, 0, 0, false);
					return;
				}
				else if (WordType == WordTypeKey.IntegerConstant || WordType == WordTypeKey.DecimalConstant)
				{
					DataType = new DataTypeInfo(eDBType.NUMERIC, 0, 0, false);
					return;
				}
			}*/

            TypedNameInfo typedInfo = nameInfo as TypedNameInfo;
            if (typedInfo != null)
                // Clone the datatype
                DataType = new DataTypeInfo(typedInfo.DataType);

        }

        private void StoreControl(KBObject kbObject)
        {
            if (!WordType.CanHaveUIControl())
                return;

            ControlName = ObjectContextCache.GetObjectCache(kbObject).ControlNames.GetValueByExactName(TextLowercase);
        }

        public bool FixOldBoolOperator(TokenInfo previous, TokenInfo next)
        {
            if (previous.WordType == WordTypeKey.Keyword && WordType == WordTypeKey.OtherMember &&
                    next.WordType == WordTypeKey.Keyword && previous.TextLowercase == "." && next.TextLowercase == ".")
            {
                if (KeywordGx.BOOLEAN_OPERATORS.Contains(TextLowercase))
                {
                    // Replace wrong token and remove points
                    WordType = WordTypeKey.Keyword;
                    return true;
                }
            }
            return false;
        }

		/// <summary>
		/// Get a text representation of the token KBObject type. It's an empty string if the token is not a KBObject
		/// </summary>
		public string KbObjectTypeText
        {
            get
            {
                if(WordType == WordTypeKey.KbObject)
                {
					string typeName = ObjClassLsi.GetClassName(KbObjectType);
					if (typeName != null)
						return typeName;
                }
                return string.Empty;
			}
        }

        public bool IsTrainableAsOutput
        {
            get
            {
                return WordType.IsTrainable() && GxTokenType != TokenKeysEx.OperatorToken && GxTokenType != TokenKeysEx.CloseParenthesisToken
                    && GxTokenType != TokenKeysEx.OpenParenthesisToken && GxTokenType != TokenKeysEx.OperatorToken &&
                    GxTokenType != TokenKeysEx.PunctuationToken && GxTokenType != TokenKeysEx.CloseCurlyBraceToken &&
                    GxTokenType != TokenKeysEx.CloseSquareBraceToken && GxTokenType != TokenKeysEx.OpenCurlyBraceToken &&
                    GxTokenType != TokenKeysEx.OpenSquareBraceToken;
            }
        }

        /// <summary>
        /// Normalized token text: lowercarse, with no initial ampersand. Empty string if text was not stored
        /// </summary>
        public string NormalizedText
        {
            get
            {
                if (TextLowercase == null)
                    return string.Empty;
                return TokenGx.NormalizeName(TextLowercase, false);
            }
        }

        /// <summary>
        /// Token text with original letters case, no initial ampersand. Empty string if text was not stored
        /// </summary>
        public string TextNoAmpersand
        {
            get
            {
                if (Text == null)
                    return string.Empty;
                if (!Text.StartsWith("&"))
                    return Text;
                return Text.Substring(1);
            }
        }

        /// <summary>
        /// If the token has a UI control, the control type name (original case). If it has no control, it's null
        /// </summary>
        public string ControlNameType
        {
            get { return ControlName == null ? string.Empty : ControlName.ControlType; }
        }

        /// <summary>
        /// null if the token cannot have control (ex. keywords). Otherwise true/fase if the token has
        /// an associated UI control or not
        /// </summary>
        public bool? IsControl
        {
            get
            {
                if (!WordType.CanHaveUIControl())
                    return null;
                return ControlName != null;
            }
        }

		public override string ToString()
		{
            return ToString(null);
        }

        /// <summary>
        /// Get text representation for token
        /// </summary>
        /// <param name="model">If not null, inputs and context for model inputs for this token will be added. If null,
        /// it will be ignored</param>
        /// <returns>Text representation</returns>
        public string ToString(DataInfo model)
		{
            string txt = "{\"" + TextLowercase + "\", " + WordType.ToString();
            if (DataType.Type != eDBType.NONE)
                txt += ", " + DataType.ToString();
            if (!string.IsNullOrEmpty(KbObjectTypeText))
                txt += ", " + KbObjectTypeText;
            string ctx = Context?.ToString();
            if (!string.IsNullOrEmpty(ctx))
                txt += ", " + ctx;
            txt += "}";

            if(model != null)
			{
                // Print model input/context for this token
                txt += " / " + ModelValues(model, model.SequenceColumns);
                if(Context != null)
                    txt += " / " + ModelValues(model, model.ContextColumns);
			}

            return txt;
        }

		private string ModelValues(DataInfo model, List<string> modelColumns)
		{
            List<string> columns = new List<string>();
            foreach (ColumnInfo col in modelColumns.Select(c => model.GetColumnDefinition(c)))
			{
				string debugValue = col.GetCsvDebugValue(this, Context);
				if (!string.IsNullOrEmpty(debugValue))
					columns.Add($"\"{col.Name}\": {debugValue}");
			}
            return "{" + string.Join(", ", columns.ToArray()) + "}";
        }
	}
}

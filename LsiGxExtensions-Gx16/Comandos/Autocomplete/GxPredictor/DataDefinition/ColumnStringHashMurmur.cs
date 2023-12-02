using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition
{
    /// <summary>
    /// Extract a int hash from strings. Murmur hash implementation
    /// </summary>
    [DataContract]
    class ColumnStringHashMurmur : ColumnInfo
    {
        /// <summary>
        /// Maximum value for the hash
        /// An extra value will be added for null value, and optionally, other for empty string
        /// </summary>
        [DataMember]
        public int MaxValue;

        /// <summary>
        /// Add an extra hash for empty string?. If false, empty string will be handled as a null value
        /// </summary>
        [DataMember]
        public bool AddValueForEmptyString = false;

        /// <summary>
        /// Function to extract the string to hash
        /// </summary>
        public Func<TokenInfo, TokenContext, string> FeatureExtract;

		/// <summary>
		/// The function to extract feature from token/context, if there is. If not, it's null
		/// </summary>
		public override object FeatureExtractFunction { get { return FeatureExtract; } set { FeatureExtract = value as Func<TokenInfo, TokenContext, string>; } }

		public ColumnStringHashMurmur(string name, int maxValue, bool addValueForEmptyString=false) : base(name)
        {
            MaxValue = maxValue;
            AddValueForEmptyString = addValueForEmptyString;
        }

        public ColumnStringHashMurmur(string name, ColumnStringHashMurmur sharedLabels) : base(name, sharedLabels.Name)
		{
            MaxValue = sharedLabels.MaxValue;
            AddValueForEmptyString = sharedLabels.AddValueForEmptyString;
        }

        private List<string> _Labels;

		// <summary>
		/// Column labels. CSV numbers on column will be indexes to these values
		/// </summary>
		[DataMember]
        override public List<string> Labels
        {
            get
            {
				if (_Labels != null)
					return _Labels;

				_Labels = new List<string>();
				_Labels.Add("(null)");
                for (int i = 0; i < (MaxValue-1); i++)
					_Labels.Add("#" + i.ToString());
                if (AddValueForEmptyString)
					_Labels.Add("(empty)");
                return _Labels;
            }
        }

        public int GetHash(string text)
        {
            // zero is reserved for null
            if (text == null)
                return 0;
            else if (text == string.Empty)
            {
                if (AddValueForEmptyString)
                    return MaxValue;
                else
                    return 0;
            }

            uint hash = MurmurHash2.Hash(text);
            return (int)(hash % (MaxValue-1)) + 1;
        }

		/// <summary>
		/// Get a index for the label feature
		/// </summary>
		/// <param name="token">Token from which extract the label feature. It could be null for token to predict</param>
		/// <param name="context">Context for token to predict</param>
		/// <param name="addNewValues">True if we should add the token feature label, if it was not found. Only for training</param>
		/// <returns>The index for the token label. If addNewValues is false and label was not found, it will return -1</returns>
		override public int GetIntValue(TokenInfo token, TokenContext context, bool addNewValues)
        {
            return GetHash(FeatureExtract(token, context));
        }

        override public string GetCsvDebugValue(TokenInfo token, TokenContext context)
        {
			string text = FeatureExtract(token, context);
			int value = GetHash(text);
			if (string.IsNullOrEmpty(text))
				text = string.Empty;
			else
				text += " - ";
			return text + ( value < 0 ? "UNKNOWN!" : Labels[value] );
		}
    }
}

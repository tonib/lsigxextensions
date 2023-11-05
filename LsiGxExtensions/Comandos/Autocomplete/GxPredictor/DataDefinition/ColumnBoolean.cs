using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition
{
    /// <summary>
    /// Boolea feature column
    /// </summary>
    [DataContract]
    class ColumnBoolean : ColumnInfo
    {
        /// <summary>
        /// Function to extract feature
        /// </summary>
        public Func<TokenInfo, TokenContext, bool?> FeatureExtract;

		/// <summary>
		/// The function to extract feature from token/context, if there is. If not, it's null
		/// </summary>
		public override object FeatureExtractFunction { get { return FeatureExtract; } set { FeatureExtract = value as Func<TokenInfo, TokenContext, bool?>; } }

		/// <summary>
		/// Column can have a null value?
		/// </summary>
		[DataMember]
        public bool Nullable;

        public ColumnBoolean(string name, bool nullable=false) : base(name)
        {
            Nullable = nullable;
        }

        override public List<string> Labels
        {
            get {
                List<string> labels = new List<string>();
                labels.Add(Boolean.FalseString);
                labels.Add(Boolean.TrueString);
                if (Nullable)
                    labels.Add("null");

                return labels;
            }
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
            bool? value = FeatureExtract(token, context);
			if (value == null)
			{
				if (!Nullable)
					throw new Exception("Column " + Name + " is not nullable, but FeatureExtract returned null");
				return 2;
			}
			else
				return (bool)value ? 1 : 0;
        }

        override public string GetCsvDebugValue(TokenInfo token, TokenContext context)
        {
            bool? value = FeatureExtract(token, context);
            if (value == null)
                return string.Empty;
            else
                return (bool)value ? bool.TrueString : bool.FalseString;
        }

    }
}

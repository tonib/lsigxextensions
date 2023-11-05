using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition
{
    /// <summary>
    /// Stores a categorized positive integer value (>= 0)
    /// </summary>
    [DataContract]
    class ColumnIntCategory : ColumnInfo
    {

        /// <summary>
        /// Function to extract int feature to categorize
        /// </summary>
        public Func<TokenInfo, TokenContext, int?> FeatureExtract;

		/// <summary>
		/// The function to extract feature from token/context, if there is. If not, it's null
		/// </summary>
		public override object FeatureExtractFunction { get { return FeatureExtract; } set { FeatureExtract = value as Func<TokenInfo, TokenContext, int?>; } }

		/// <summary>
		/// Column can have a null value?
		/// </summary>
		[DataMember]
        public bool Nullable;

        /// <summary>
        /// Values division. Each values is the maximum value for the category
        /// </summary>
        [DataMember]
        int[] Buckets;

        /// <summary>
        /// Column labels. CSV numbers on column will be indexes to these values
        /// </summary>
        [DataMember]
        override public List<string> Labels
        {
            get
            {
                if(_Labels == null)
                {
                    _Labels = new List<string>();
                    for(int i=0; i<Buckets.Length; i++)
                    {
                        int min = (i == 0 ? 0 : Buckets[i - 1] + 1);
                        int max = Buckets[i];
                        _Labels.Add(min == max ? min.ToString() : min + "-" + max);
                    }
                    _Labels.Add(">" + Buckets[Buckets.Length - 1]);
                    if (Nullable)
                        _Labels.Add("null");
                }

                return _Labels;
            }
        }
        private List<string> _Labels;

        public ColumnIntCategory(string name, int[] buckets, bool nullable=false) : base(name)
        {
            Buckets = buckets;
            Nullable = nullable;
        }

        public ColumnIntCategory(string name, ColumnIntCategory sharedLabels) : base(name, sharedLabels.Name)
		{
            Buckets = sharedLabels.Buckets;
            Nullable = sharedLabels.Nullable;
        }

        public int ToBucketIndex(int? value)
        {
			if (value == null)
			{
				if (!Nullable)
					throw new Exception("Column " + Name + " is not nullable, but FeatureExtract returned null");
				return Buckets.Length + 1;
			}

            for (int i = 0; i < Buckets.Length; i++)
            {
                if (value <= Buckets[i])
                    return i;
            }
            return Buckets.Length;
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
            return ToBucketIndex(FeatureExtract(token, context));
        }

        override public string GetCsvDebugValue(TokenInfo token, TokenContext context) {
            int? value = FeatureExtract(token, context);
            if (value == null)
                return string.Empty;

            return value + " [" + Labels[ToBucketIndex(value)] + "]";
        }

    }
}

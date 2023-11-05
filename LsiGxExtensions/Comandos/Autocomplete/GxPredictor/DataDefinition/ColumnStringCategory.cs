using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition
{
    /// <summary>
    /// Stores an indexed string category
    /// </summary>
    [DataContract]
    class ColumnStringCategory : ColumnInfo
    {

        protected IndexedList<string> IndexedLabels;

        public Func<TokenInfo, TokenContext, string> FeatureExtract;

		/// <summary>
		/// The function to extract feature from token/context, if there is. If not, it's null
		/// </summary>
		public override object FeatureExtractFunction { get { return FeatureExtract; } set { FeatureExtract = value as Func<TokenInfo, TokenContext, string>; } }

		public ColumnStringCategory(string name, ColumnStringCategory sharedLabelsColumn = null) : base(name) 
        {
            if (sharedLabelsColumn == null)
                IndexedLabels = new IndexedList<string>();
            else
                BindLabels(sharedLabelsColumn);
        }

        override public List<string> Labels
        {
            get { return IndexedLabels.List; }
            set
            {
                // It will be null after deserialization
                if (IndexedLabels == null)
                    IndexedLabels = new IndexedList<string>();
                IndexedLabels.List = value;
            }
        }

		/// <summary>
		/// Get a index for the label feature
		/// </summary>
		/// <param name="token">Token from which extract the label feature. It could be null for token to predict</param>
		/// <param name="context">Context for token to predict</param>
		/// <param name="addNewValues">True if we should add the token feature label, if it was not found. Only for training</param>
		/// <returns>The index for the token label. If addNewValues is false and label was not found, it will return -1</returns>
		private int GetIdx(string value, bool addNewValues)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            return IndexedLabels.GetIndex(value, addNewValues);
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
            return GetIdx(FeatureExtract(token, context), addNewValues);
        }

        override public string GetCsvDebugValue(TokenInfo token, TokenContext context)
        {
            return FeatureExtract(token, context);
        }

        /// <summary>
        /// Use labels from other column, sharing labels with it
        /// </summary>
        /// <param name="sharedLabelsColumn">Column with shared labels to use</param>
        public void BindLabels(ColumnStringCategory sharedLabelsColumn)
		{
            IndexedLabels = sharedLabelsColumn.IndexedLabels;
            SharedLabelsId = sharedLabelsColumn.Name;
        }
    }
}

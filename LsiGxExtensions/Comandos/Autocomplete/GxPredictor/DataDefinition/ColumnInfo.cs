using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition
{
    /// <summary>
    /// Information about a token column on exported CSV files
    /// </summary>
    [DataContract]
    [KnownType(typeof(ColumnBoolean))]
    [KnownType(typeof(ColumnEnum<WordTypeKey>))]
    [KnownType(typeof(ColumnIntCategory))]
    [KnownType(typeof(ColumnStringCategory))]
    [KnownType(typeof(ColumnStringHashMurmur))]
    public abstract class ColumnInfo
    {
        /// <summary>
        /// Column name
        /// </summary>
        [DataMember]
        public string Name;

        /// <summary>
        /// Name of shared labels column that have the same labels
        /// </summary>
        [DataMember]
        public string SharedLabelsId { get; protected set; }

        /// <summary>
        /// If > 0, this column will be embedded, with this dimension
        /// </summary>
        [DataMember]
        public int EmbeddableDimension;

        /// <summary>
        /// Column labels. CSV numbers on column will be indexes to these values
        /// </summary>
        [DataMember]
        virtual public List<string> Labels 
        {
            get { return null; }
            set { }
        }

		/// <summary>
		/// Get a index for the label feature
		/// </summary>
		/// <param name="token">Token from which extract the label feature. It could be null for token to predict</param>
		/// <param name="context">Context for token to predict</param>
		/// <param name="addNewValues">True if we should add the token feature label, if it was not found. Only for training</param>
		/// <returns>The index for the token label. If addNewValues is false and label was not found, it will return -1</returns>
		virtual public int GetIntValue(TokenInfo token, TokenContext context, bool addNewValues)
        {
            return -1;
        }

        public string GetCsvValue(TokenInfo token, TokenContext context)
        {
            return GetIntValue(token, context, true).ToString();
        }

        virtual public string GetCsvDebugValue(TokenInfo token, TokenContext context)
        {
            return GetCsvValue(token, context);
        }

		/// <summary>
		/// The function to extract feature from token/context, as object
		/// </summary>
		public abstract object FeatureExtractFunction { get; set; }

		public ColumnInfo() { }

        public ColumnInfo(string name, string sharedLabelsId = null)
        {
            Name = name;
            SharedLabelsId = sharedLabelsId;
        }
        
    }
}

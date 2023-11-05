using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition
{
	// It seems restricting a generic to a Enum type is not available yet (https://stackoverflow.com/questions/79126/create-generic-method-constraining-t-to-an-enum)
	// So here we expect FeatureExtract to return an int?
	[DataContract]
    internal class ColumnEnum<T> : ColumnInfo
	{

		private List<string> _Labels;

		override public List<string> Labels
        {
            get {
				if (_Labels == null)
				{
					_Labels = Enum.GetNames(typeof(T)).ToList();
					if (Nullable)
						_Labels.Add("(null)");
				}
				return _Labels;
			}
            set {}
        }

		/// <summary>
		/// Column can have a null value?
		/// </summary>
		[DataMember]
		public bool Nullable;

		public Func<TokenInfo, TokenContext, int?> FeatureExtract;

		/// <summary>
		/// The function to extract feature from token/context, if there is. If not, it's null
		/// </summary>
		public override object FeatureExtractFunction { get { return FeatureExtract; } set { FeatureExtract = value as Func<TokenInfo, TokenContext, int?>; } }

		public ColumnEnum(string name, bool nullable = false) : base(name)
		{
			Nullable = nullable;
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
            int? featureIntValue = FeatureExtract(token, context);
			if (featureIntValue == null)
			{
				if (!Nullable)
					throw new Exception("Column " + Name + " is not nullable, but FeatureExtract returned null");
				return Labels.Count - 1;
			}
			else
				return (int)featureIntValue;
		}

        override public string GetCsvDebugValue(TokenInfo token, TokenContext context)
        {
			return Labels[GetIntValue(token, context, false)];
        }
    }
}

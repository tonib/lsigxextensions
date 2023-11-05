using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition
{
	/// <summary>
	/// Debug column to return a generic string with a token value
	/// </summary>
	[DataContract]
	class ColumnDebugString  : ColumnInfo
    {

        public Func<TokenInfo, TokenContext, string> FeatureExtract;

		/// <summary>
		/// As this is not a real data value, just save something as label, to keep compatibility with other columns
		/// </summary>
		override public List<string> Labels
		{
			get { return new List<string>(new string[] { "DEBUGCOLUMN" }); }
			set { }
		}

		/// <summary>
		/// The function to extract feature from token/context, if there is. If not, it's null
		/// </summary>
		public override object FeatureExtractFunction { get { return FeatureExtract; } set { FeatureExtract = value as Func<TokenInfo, TokenContext, string>; } }

		public ColumnDebugString(string name) : base(name) { }

        override public string GetCsvDebugValue(TokenInfo token, TokenContext context)
        {
            return FeatureExtract(token, context);
        }
    }
}

using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.PredictionGeneration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor
{
    /// <summary>
    /// List of code tokens
    /// </summary>
    public class TokensList : List<TokenInfo>
    {

		public TokensList() { }

		public TokensList(int capacity): base(capacity) { }

        public void WriteToCsv(DataInfo dataInfo, StreamWriter writer)
        {
            ForEach(x => writer.WriteLine(dataInfo.ToCsvRow(x, true)));
        }

		public override string ToString()
		{
            return ToString(null);
        }

        /// <summary>
        /// Text representation
        /// </summary>
        /// <param name="model">If not null, info about model input for tokens will be added</param>
        /// <returns>Text representation</returns>
        public string ToString(DataInfo model)
        {
            return Count + " tokens:" + Environment.NewLine +
                string.Join(Environment.NewLine + Environment.NewLine, 
                    this.Select((t, idx) => idx + ": " + t.ToString(model)).ToArray());
        }
    }

}

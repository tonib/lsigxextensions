using Artech.Architecture.Common.Services;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.PredictionGeneration
{
    /// <summary>
    /// Sequence input data to feed the predictor
    /// </summary>
    /// <remarks>
    /// It's "dinamyc" (not hardcoded properties names)
    /// to allow support multiple prediction versions.
    /// </remarks>
    public class PredictorInput 
    {
        /// <summary>
        /// Sequence columns
        /// </summary>
        Dictionary<string, List<int>> Sequences;

        /// <summary>
        /// Context columns
        /// </summary>
        Dictionary<string, List<int>> Contexts = new Dictionary<string, List<int>>();

		/// <summary>
		/// True if context should be feeded for each timestep, and token to predict. False if context should be feeded
		/// only for token to predict
		/// </summary>
		bool FeedContextAllTimesteps;

		/// <summary>
		/// Input tokens
		/// </summary>
		public TokensList Tokens;

		/// <summary>
		/// Input context
		/// </summary>
		public TokenContext Context;

		/// <summary>
		/// Creates model input
		/// </summary>
		/// <param name="dataInfo">Model information</param>
		/// <param name="tokens">Tokens previous to the cursor</param>
		/// <param name="context">Context in cursor position</param>
		public PredictorInput(DataInfo dataInfo, TokensList tokens, TokenContext context)
		{
			FeedContextAllTimesteps = dataInfo.FeedContextAllTimesteps;
			Tokens = tokens;
			Context = context;

			// Create empty senquences:
			Sequences = new Dictionary<string, List<int>>();
			foreach (string seqColumnName in dataInfo.SequenceColumns)
				Sequences[seqColumnName] = new List<int>(dataInfo.SequenceLength);
			foreach (string ctxColumnName in dataInfo.ContextColumns)
				Contexts[ctxColumnName] = new List<int>(dataInfo.SequenceLength+1);

			// Feed sequence/context inputs for previous tokens
			foreach (TokenInfo token in tokens)
			{
				// Sequence features
				foreach (string columnName in dataInfo.SequenceColumns)
					FeedValue(dataInfo, token, null, columnName);
				if (FeedContextAllTimesteps)
				{
					// Feed context for this token
					foreach (string columnName in dataInfo.ContextColumns)
						FeedValue(dataInfo, null, token.Context, columnName);
				}
			}

			// Feed context for token to predict
			foreach (string columnName in dataInfo.ContextColumns)
				FeedValue(dataInfo, null, context, columnName);
		}

		private void FeedValue(DataInfo dataInfo, TokenInfo token, TokenContext context, string columnName)
		{
			int value = dataInfo.GetColumnDefinition(columnName).GetIntValue(token, context, false);

			if (value < 0)
			{
#if DEBUG
				object sourceObject = (token != null ? (object)token : (object)context);
				CommonServices.Output.AddWarningLine(sourceObject.ToString() + ": unknown value for feature " + columnName);
#endif
				value = 0;
			}
			// TODO: It would be nice to have a safety check here for value >= columnInfo.Labels.Count, but as labels are generated dynamically
			// TODO: This could be expensive

			(token != null ? Sequences : Contexts)[columnName].Add(value);
		}

		/// <summary>
		/// Get probabilities for a given output column name.
		/// </summary>
		/// <param name="columnName">The output column name. This will raise an exception if column name don't exists</param>
		/// <returns>Probabilities for each output column label</returns>
		public List<int> GetColumnValues(string columnName)
        {
            List<int> values;
            if (Sequences.TryGetValue(columnName, out values))
                return values;
            return Contexts[columnName];
        }

		/// <summary>
		/// Returns the JSON to feed as model input
		/// </summary>
		/// <returns>JSON to feed as model input</returns>
		public string ToJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('{');

			AddFeatureValues(sb, Sequences, true);
			sb.Append(',');
			AddFeatureValues(sb, Contexts, FeedContextAllTimesteps);

			sb.Append('}');
            return sb.ToString();
        }

		static private void AddFeatureValues(StringBuilder sb, Dictionary<string, List<int>> features, bool isSequence)
		{
			bool first = true;

			foreach(KeyValuePair<string, List<int>> s in features)
			{
				if (first)
					first = false;
				else
					sb.Append(',');

				sb.Append('"');
				sb.Append(s.Key);
				sb.Append("\":");
				if(isSequence)
				{
					sb.Append('[');
					sb.Append(string.Join(",", s.Value.Select(x => x.ToString()).ToArray()));
					sb.Append(']');
				}
				else
					sb.Append(s.Value[0]);
			}
		}
    }
}

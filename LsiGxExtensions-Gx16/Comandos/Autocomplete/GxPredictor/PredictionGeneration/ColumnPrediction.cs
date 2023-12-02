using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.PredictionGeneration
{
    /// <summary>
    /// A feature prediction
    /// </summary>
    public class ColumnPrediction
    {

		/// <summary>
		/// Column name
		/// </summary>
		public string Name;

		/// <summary>
		/// Probability of each feature class. Index array is the class. Value array is the probability in [0-1]
		/// </summary>
		public double[] Probabilities;

		public ColumnPrediction(string name, double[] probabilities)
		{
			this.Name = name;
			this.Probabilities = probabilities;
		}

		/// <summary>
		/// Get feature classes sorted by it's probability
		/// </summary>
		/// <returns>Class indices, sorted by probability, from most probable to less probable</returns>
		public int[] GetSortedProbIndices()
        {
            return GetSortedClassesWithProb()
                .Select(x => x.Key)
                .ToArray();
        }

        /// <summary>
        /// Get feature classes sorted by it's probability
        /// </summary>
        /// <returns>Class indices, sorted by probability, from most probable to less probable. Key is the
        /// class index, value is the probability</returns>
        public IEnumerable<KeyValuePair<int, double>> GetSortedClassesWithProb()
        {
            return Probabilities
                .Select((prob, i) => new KeyValuePair<int, double>(i, prob))
                .OrderByDescending(x => x.Value);
        }
    }
}

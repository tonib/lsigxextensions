using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.PredictionGeneration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.PredictionBindings
{
    /// <summary>
    /// Makes hashes combinations, and return them sorted by probability
    /// </summary>
    class HashesCombinator : IEnumerable<int[]>
    {

        /// <summary>
        /// Most probable hashes for each word part. Index 1 is the hash column index.
        /// Index 2 is for the set of most probable hashes on that column
        /// </summary>
        private KeyValuePair<int, double>[][] ClassesPerColumn;

        private int EmptyStringHash;

        public HashesCombinator(PredictionResult prediction, ColumnPrediction[] hashNameColumns, int nMaxElementsPerColumn)
        {
            // Get hashes of each dimension, sorted by probability
            // Index 1 is the column index, index 2 is the hash index
            ClassesPerColumn = new KeyValuePair<int, double>[hashNameColumns.Length][];
            for (int i = 0; i < ClassesPerColumn.Length; i++)
            {
                // Ignore hash zero: It's reserved for "null string", that is, tokens not not hash: keywords, operators, etc
                ClassesPerColumn[i] = hashNameColumns[i].GetSortedClassesWithProb()
                    .Where(x => x.Key != 0)
                    .Take(nMaxElementsPerColumn).ToArray();
            }

			// Get the hash for an empty string:
			EmptyStringHash = prediction.KbInfo.DataInfo.GetNameHashes(string.Empty)[0];
		}

        private KeyValuePair<int[], double> GetHashWithProbability(IEnumerable<KeyValuePair<int, double>> hashesWithProb)
        {
            int[] hashes = hashesWithProb.Select(x => x.Key).ToArray();
            double prob = hashesWithProb.Select(x => x.Value).Aggregate(1.0 , (accumulate, value) => accumulate * value );
            return new KeyValuePair<int[], double>(hashes, prob);            
        }

        private bool IsValidCombination(int[] hashes)
        {
            // Combinations with empty string in the middle are not valid ( ex. [hashX, "" , hashY, hashZ] )
            bool emptyFound = false;
            for(int i=0;i<hashes.Length;i++)
            {
                if (emptyFound)
                {
                    if (hashes[i] != EmptyStringHash)
                        // String after empty: Not valid
                        return false;
                }
                else if(hashes[i] == EmptyStringHash)
                    emptyFound = true;
            }
            return true;
        }

        public IEnumerator<int[]> GetEnumerator()
        {
            // All combinations:
            IEnumerable<int[]> combinations = CartesianProduct(ClassesPerColumn)
                // Get the hashes combined on an array, with the final probability of the combination
                .Select(x => GetHashWithProbability(x))
                // Check combination is valid
                .Where(x => IsValidCombination(x.Key))
                // First most probable combinations
                .OrderByDescending(x => x.Value)
                // Return only the hashes combination
                .Select(x => x.Key);

            foreach (int[] hashes in combinations)
                yield return hashes;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // Code taken from https://stackoverflow.com/questions/25643382/cartesian-products-with-n-number-of-list/25643434
        private IEnumerable<IEnumerable<T>> CartesianProduct<T>(IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            IEnumerable<IEnumerable<T>> result = emptyProduct;
            foreach (IEnumerable<T> sequence in sequences)
            {
                result = from accseq in result from item in sequence select accseq.Concat(new[] { item });
            }
            return result;
        }
    }
}

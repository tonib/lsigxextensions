using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor
{
    /// <summary>
    /// List with fast value search
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public class IndexedList<T>
    {
        private List<T> _List = new List<T>();

        private Dictionary<T, int> Indices = new Dictionary<T, int>();

        [DataMember]
        public List<T> List
        {
            get { return _List; }
            set
            {
                _List = new List<T>(value.Count);
                Indices.Clear();
                foreach (T item in value)
                    GetIndex(item);
            }
        }

		/// <summary>
		/// Get index for a value in this list
		/// </summary>
		/// <param name="item">Value to search</param>
		/// <param name="addIfNotFound">If true, and value is not in list, it will be added automatically</param>
		/// <returns>Value index. If addIfNotFound is false and value was not found, it will return -1</returns>
		public int GetIndex(T item, bool addIfNotFound = true)
        {
            int idx;
            if( !Indices.TryGetValue(item, out idx) )
            {
                if (!addIfNotFound)
                    return -1;

                _List.Add(item);
                idx = _List.Count - 1;
                Indices.Add(item, idx);
            }
            return idx;
        }

        public T GetValue(int idx)
        {
            return _List[idx];
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.GxClassExtensions
{
    /// <summary>
    /// HashSet extensions
    /// </summary>
    static public class HashSetExtensions
    {

        static public void LsiAddRange<T>(this HashSet<T> set, IEnumerable<T> items)
        {
            foreach (T item in items)
                set.Add(item);
        }

        public static HashSet<T> LsiToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.Reflection
{
    public class SerializableKeyValue<K, V>
    {
        public K Key { get; set; }

        public V Value { get; set; }

        public SerializableKeyValue() { }

        public SerializableKeyValue(K key, V value)
        {
            Key = key;
            Value = value;
        }

        public SerializableKeyValue(KeyValuePair<K, V> pair)
        {
            Key = pair.Key;
            Value = pair.Value;
        }

    }
}

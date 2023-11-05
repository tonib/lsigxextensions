using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace LSI.Packages.Extensiones.Utilidades.Reflection
{
    /// <summary>
    /// Tools to serialize a dictionary.
    /// </summary>
    [XmlRoot("SerializableDictionary")]
    public class SerializableDictionary<K, V>
    {

        [XmlArray("Items")]
        [XmlArrayItem("Item")]
        public List<SerializableKeyValue<K, V>> Content = new List<SerializableKeyValue<K, V>>();

        public SerializableDictionary() { }

        public SerializableDictionary(Dictionary<K, V> dictionary)
        {
            // XmlSerializer does not support dictionaries:
            dictionary.ToList().ForEach(x => Content.Add(new SerializableKeyValue<K, V>(x)));
        }

        public void Save(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            using (TextWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, this);
            }
        }

        static public SerializableDictionary<K,V> Load(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<K, V>));
            using( TextReader reader = new StreamReader(filePath) ) 
            {
                SerializableDictionary<K, V> d =
                    (SerializableDictionary<K, V>)serializer.Deserialize(reader);
                return d;
            }           
        }

        public Dictionary<K, V> ToDictionary()
        {
            Dictionary<K, V> d = new Dictionary<K,V>();
            Content.ForEach(x => d.Add(x.Key, x.Value));
            return d;
        }

    }
}

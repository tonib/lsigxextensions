using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.Threading;
using Artech.Architecture.UI.Framework.Services;
using Artech.Architecture.UI.Framework.Packages;
using Artech.Common.Framework.Selection;
using Artech.Architecture.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using Artech.Udm.Framework;
using System.IO;
using System.Xml.Serialization;
using LSI.Packages.Extensiones.Utilidades.Reflection;

namespace LSI.Packages.Extensiones.Utilidades.CSharpWin
{
    /// <summary>
    /// Singleton to store a cache generated source file names by object.
    /// </summary>
    /// <remarks>
    /// This is useful to improve the performance of RSP files repair functionallity
    /// </remarks>
    public class GeneratedSourceFilesCache
    {

        /// <summary>
        /// EntityKey is not serializable because it does not contains a default constructor.
        /// So we need this ugly thing.
        /// </summary>
        public class SerializableEntityKey
        {

            public int Id;
            public Guid Type;

            public SerializableEntityKey()
            { }

            public SerializableEntityKey(EntityKey key) 
            {
                Id = key.Id;
                Type = key.Type;
            }

            public override bool Equals(object obj)
            {
                SerializableEntityKey k = obj as SerializableEntityKey;
                if (k == null)
                    return false;
                return k.Id == Id && k.Type == Type;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode() + Type.GetHashCode();
            }

            /// <summary>
            /// Get the genexus key
            /// </summary>
            public EntityKey Key
            {
                get { return new EntityKey(Type, Id); }
            }
        }

        /// <summary>
        /// Kbase cache file name.
        /// </summary>
        private const string CACHEFILENAME = "SourceFilesCache.xml";

        /// <summary>
        /// Singleton instance
        /// </summary>
        static private GeneratedSourceFilesCache _Cache;

        /// <summary>
        /// Cache unique instance
        /// </summary>
        static public GeneratedSourceFilesCache Cache(KnowledgeBase kb)
        {
            if (_Cache == null)
                _Cache = new GeneratedSourceFilesCache();
            _Cache.ReloadCache(kb);

            return _Cache;
        }

        /// <summary>
        /// Current KBase directory. Used to detect kbase changes
        /// </summary>
        private string KBasePath;

        /// <summary>
        /// Source files cache. Key is the object entity key and value are the generated source files.
        /// </summary>
        private Dictionary<SerializableEntityKey, ObjectSourceFiles> SourceFilesCache =
            new Dictionary<SerializableEntityKey, ObjectSourceFiles>();

        private void ReloadCache(KnowledgeBase kb)
        {
            if (KBasePath != null && KBasePath.ToLower() == kb.Location.ToLower())
                // Kbase not changed
                return;

            // Current kb location
            KBasePath = kb.Location;

            // Cache cleanup
            SourceFilesCache = new Dictionary<SerializableEntityKey, ObjectSourceFiles>();

            string filePath = Entorno.GetLsiExtensionsFilePath(kb, CACHEFILENAME);
            if (!File.Exists(filePath))
                // There is no stored kb cache
                return;

            try
            {
                // Read cache file (XmlSerializer does not support dictionaries)
                SourceFilesCache = SerializableDictionary<SerializableEntityKey, ObjectSourceFiles>
                    .Load(filePath)
                    .ToDictionary();
            }
            catch
            { }
        }

        public void Save(KnowledgeBase kb)
        {
            // XmlSerializer does not support dictionaries:
            SerializableDictionary<SerializableEntityKey, ObjectSourceFiles> d =
                new SerializableDictionary<SerializableEntityKey, ObjectSourceFiles>(SourceFilesCache);
            string filePath = Entorno.GetLsiExtensionsFilePath(kb, CACHEFILENAME);
            d.Save(filePath);
        }

        public ObjectSourceFiles GetSourceFiles(KBObject o)
        {
            ObjectSourceFiles info;
            SerializableEntityKey k = new SerializableEntityKey(o.Key);
            if (!SourceFilesCache.TryGetValue(k, out info))
            {
                info = new ObjectSourceFiles(o);
                SourceFilesCache.Add(k, info);
            }
            info.RecalculateIfNeeded(o);
            return info;
        }

        /// <summary>
        /// Remove objects cached information
        /// </summary>
        public void Clean(KnowledgeBase kb)
        {
            SourceFilesCache = new Dictionary<SerializableEntityKey, ObjectSourceFiles>();

            string filePath = Entorno.GetLsiExtensionsFilePath(kb, CACHEFILENAME);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}

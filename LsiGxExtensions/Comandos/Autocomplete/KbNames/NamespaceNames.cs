using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.Common.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames
{
    /// <summary>
    /// Stores a trie for each Genexus namespace
    /// </summary>
    class NamespaceNames
    {
        private Dictionary<string, Trie<ObjectNameInfo>> NamespacesTries = new Dictionary<string, Trie<ObjectNameInfo>>();

        /// <summary>
        /// Special namespace for language keywords
        /// </summary>
        public const string KEYWORDSNAMESPACE = "KEYWORDS";

        public Trie<ObjectNameInfo> GetTrie(Guid objectType)
        {
            KBObjectDescriptor descriptor = KBObjectDescriptor.Get(objectType);
            if (descriptor == null || descriptor.Namespace == null)
                return null;

            return GetTrie(descriptor.Namespace);
        }

        public Trie<ObjectNameInfo> GetTrie(string nSpace)
        {
            Trie<ObjectNameInfo> trie;
            if (!NamespacesTries.TryGetValue(nSpace, out trie))
            {
                trie = new Trie<ObjectNameInfo>();
                NamespacesTries.Add(nSpace, trie);
            }

            return trie;
        }

        public KbObjectNameInfo Add(KBObject o)
        {
            KbObjectNameInfo name = new KbObjectNameInfo(o);
            GetTrie(o.Key.Type).Add(o.Name.ToLower(), name);
            return name;
        }

        public ObjectNameInfo Remove(KBObject o, string nameToRemove)
        {
            return GetTrie(o.Key.Type).Remove(nameToRemove);
        }

        public void Clear()
        {
            NamespacesTries.Clear();
        }

        public int Count
        {
            get { return NamespacesTries.Values.Sum(x => x.Count); }
        }

        /// <summary>
        /// Return names starting with a given prefix
        /// </summary>
        /// <param name="prefix">Prefix of names to return</param>
        /// <param name="nNamesPerNamespace">Number of names to return for each namespace. If ==0, all names
        /// will be returned</param>
        /// <returns>Names starting with the given prefix</returns>
        public IEnumerable<ObjectNameInfo> GetValuesByPrefix(string prefix, int nNamesPerNamespace = 0)
        {
            prefix = prefix.ToLower();
            foreach(Trie<ObjectNameInfo> trie in NamespacesTries.Values)
            {
                int nNamesReturned = 0;
                foreach (ObjectNameInfo name in trie.GetValuesByPrefix(prefix))
                {
                    yield return name;
                    nNamesReturned++;
                    if (nNamesPerNamespace > 0 && nNamesReturned >= nNamesPerNamespace)
                        break;
                }
            }
        }

        public List<ObjectNameInfo> GetAllByExactName(string name)
        {
            List<ObjectNameInfo> result = new List<ObjectNameInfo>();

            foreach (Trie<ObjectNameInfo> trie in NamespacesTries.Values)
            {
                ObjectNameInfo i = trie.GetValueByExactName(name);
                if (i != null)
                    result.Add(i);
            }

            return result;
        }

    }
}

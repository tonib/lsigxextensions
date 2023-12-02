using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames
{
    /// <summary>
    /// Stores names by its text hashes: One text can have a sequence of hashes, or have only one
    /// </summary>
    public class HashedNames
    {
        /// <summary>
        /// Function that computes the hashes. It must receive the name with the original case
        /// </summary>
        private Func<string, int[]> HashFunction;

        /// <summary>
        /// Trie that stores the hashed values. The inner Trie key is the name lowercase
        /// </summary>
        private GenericTrie< int , Trie<List<ObjectNameInfo>> > Values;

        public HashedNames(Func<string, int[]> hashFunction)
        {
            HashFunction = hashFunction;
            Values = new GenericTrie<int, Trie<List<ObjectNameInfo>>>();
        }

        private Trie<List<ObjectNameInfo>> GetTrie(ObjectNameInfo name, bool addIfNotExists)
        {
            // Notice this takes the hash with the original case (NOT lowercase)
            int[] hash = HashFunction(name.Name);
            Trie<List<ObjectNameInfo>> trie = Values.GetValueByExactName(hash);
            if( addIfNotExists && trie == null)
            {
                trie = new Trie<List<ObjectNameInfo>>();
                Values.Add(hash, trie);
            }
            return trie;
        }

        public void Add(ObjectNameInfo name)
        {
            Trie<List<ObjectNameInfo>> trie = GetTrie(name, true);

            string textNameLowercase = name.Name.ToLower();
            List<ObjectNameInfo> names = trie.GetValueByExactName(textNameLowercase);
            if(names == null)
            {
                names = new List<ObjectNameInfo>(2);
                trie.Add(textNameLowercase, names);
            }
            names.Add(name);
        }

        public void AddRange(IEnumerable<ObjectNameInfo> names)
        {
            foreach(ObjectNameInfo name in names)
                Add(name);
        }

        public void Remove(ObjectNameInfo name)
        {
            Trie<List<ObjectNameInfo>> trie = GetTrie(name, false);
            if (trie == null)
                return;
            string textNameLowercase = name.Name.ToLower();
            trie.Remove(textNameLowercase);
        }

        public IEnumerable<ObjectNameInfo> GetByHashAndPrefix(int[] hash, string prefix)
        {
            prefix = prefix.ToLower();

            Trie<List<ObjectNameInfo>> trie = Values.GetValueByExactName(hash);
            if (trie != null)
            {
                foreach (List<ObjectNameInfo> list in trie.GetValuesByPrefix(prefix))
                {
                    foreach (ObjectNameInfo name in list)
                        yield return name;
                }
            }
        }

    }
}

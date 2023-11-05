using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames
{
    /// <summary>
    /// Trie implementation with a generic key type
    /// </summary>
    /// <typeparam name="TKey">Key value data type</typeparam>
    /// <typeparam name="TValue">Value to store in trie</typeparam>
    public class GenericTrie<TKey, TValue>
    {
        private TrieNode<TKey, TValue> Root = new TrieNode<TKey, TValue>(default(TKey));

        public int Count { get; private set; }

        public void Add(IEnumerable<TKey> key, TValue value)
        {
            // TODO: Check duplicated values

            TrieNode<TKey, TValue> currentNode = Root;
            foreach(TKey c in key)
                currentNode = currentNode.AddChildForChar( c );

            currentNode.Value = value;

            Count++;
        }

        public TValue Remove(IEnumerable<TKey> key)
        {
            TrieNode<TKey, TValue> node = QueryNodeForPrefix(key);
            if (node == null)
                return default(TValue);

            TValue oldValue = node.TerminalValue;
            node.RemoveValue();
            Count--;
            return oldValue;
        }

        public void AddRange(IEnumerable<KeyValuePair<IEnumerable<TKey>, TValue>> range)
        {
            foreach(KeyValuePair<IEnumerable<TKey>, TValue> pair in range)
                Add(pair.Key, pair.Value);
        }

        private TrieNode<TKey, TValue> QueryNodeForPrefix(IEnumerable<TKey> prefix)
        {
            // Go to the prefix node
            TrieNode<TKey, TValue> currentNode = Root;
            foreach(TKey c in prefix)
            {
                currentNode = currentNode.QueryChildForChar(c);
                if (currentNode == null)
                    return null;
            }

            return currentNode;
        }

        public IEnumerable<TValue> GetValuesByPrefix(IEnumerable<TKey> prefix)
        {
            // Go to the prefix node
            TrieNode<TKey, TValue> node = QueryNodeForPrefix(prefix);
            if (node == null)
                return Enumerable.Empty<TValue>();

            return node.TraverseDescendantValues();
        }

        public bool ContainsKey(IEnumerable<TKey> name)
        {
            TrieNode<TKey, TValue> node = QueryNodeForPrefix(name);
            if (node == null)
                return false;
            return node.IsTerminalNode;
        }

        public TValue GetValueByExactName(IEnumerable<TKey> name)
        {
            // Go to the name node
            TrieNode<TKey, TValue> node = QueryNodeForPrefix(name);
            if (node == null)
                return default(TValue);

            return node.TerminalValue;
        }

        public void Clear()
        {
            Root = new TrieNode<TKey, TValue>(default(TKey));
        }
    }

    /// <summary>
    /// Trie implementation with string key
    /// </summary>
    /// <typeparam name="TValue">Value to store in trie</typeparam>
    class Trie<TValue> : GenericTrie<char, TValue> { }

    internal class TrieNode<TKey, TValue>
    {
        /// <summary>
        /// Node children, sorted by the next string char
        /// </summary>
        private SortedDictionary<TKey, TrieNode<TKey, TValue>> Children = null;

        /// <summary>
        /// Node has a terminal value?
        /// </summary>
        public bool IsTerminalNode;

        /// <summary>
        /// Node value
        /// </summary>
        private TValue _Value;

        private TKey Character;

        public TrieNode(TKey character)
        {
            Character = character;
        }

        public TValue Value {
            set {
                IsTerminalNode = true;
                _Value = value;
            }
        }

        public void RemoveValue()
        {
            _Value = default(TValue);
            IsTerminalNode = false;
        }

        public TrieNode<TKey, TValue> AddChildForChar(TKey c)
        {
            if (Children == null)
                Children = new SortedDictionary<TKey, TrieNode<TKey, TValue>>();

            TrieNode<TKey, TValue> child;
            if( !Children.TryGetValue(c, out child) )
            {
                child = new TrieNode<TKey, TValue>(c);
                Children.Add(c, child);
            }
            return child;
        }

        public TrieNode<TKey, TValue> QueryChildForChar(TKey c)
        {
            if (Children == null)
                return null;

            TrieNode<TKey, TValue> child;
            if (!Children.TryGetValue(c, out child))
                return null;
            return child;
        }

        public IEnumerable<TValue> TraverseDescendantValues()
        {
            if (IsTerminalNode)
                yield return _Value;

            if (Children == null)
                yield break;

            foreach (KeyValuePair<TKey, TrieNode<TKey, TValue>> child in Children)
            {
                IEnumerable<TValue> childValues = child.Value.TraverseDescendantValues();
                foreach (TValue value in childValues)
                    yield return value;
            }
        }

        public TValue TerminalValue
        {
            get
            {
                if (!IsTerminalNode)
                    return default(TValue);
                return _Value;
            }
        }
    }
}

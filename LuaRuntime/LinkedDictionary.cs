using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LuaRuntime
{
    public class LinkedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        public Dictionary<TKey, TValue> _Self;
        public IDictionary<TKey, TValue> _Parent;
        public LinkedDictionary(IDictionary<TKey, TValue> Parent)
        {
            _Parent = Parent;
            _Self = new Dictionary<TKey, TValue>();
        }
        public void Add(TKey key, TValue value)
        {
            _Self.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return _Self.ContainsKey(key) || _Parent.ContainsKey(key);
        }

        public ICollection<TKey> Keys => _Self.Keys.Concat(_Parent.Keys).ToList();
        public bool Remove(TKey key)
        {
            return _Self.Remove(key);
        }

        public bool RemoveRecursive(TKey key)
        {
            return _Self.ContainsKey(key) ? _Self.Remove(key) : _Parent.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _Self.TryGetValue(key, out value) || _Parent.TryGetValue(key, out value);
        }
        public ICollection<TValue> Values => _Self.Values.Concat(_Parent.Values).ToList();
        public TValue this[TKey key]
        {
            get => _Self.ContainsKey(key) ? _Self[key] : _Parent[key];
            set => _Self[key] = value;
        }
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _Self.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _Self.Contains(item) || _Parent.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count => _Self.Count + _Parent.Count;
        public bool IsReadOnly => throw new NotImplementedException();
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _Self.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Self.GetEnumerator();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        int ICollection<KeyValuePair<TKey, TValue>>.Count => Keys.Count;
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            _Self.Add(key, value);
        }

        bool IDictionary<TKey, TValue>.ContainsKey(TKey key)
        {
            return ContainsKey(key);
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;
        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            return Remove(key);
        }

        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)
        {
            return TryGetValue(key, out value);
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;
        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get => this[key];
            set => this[key] = value;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using UnityEngine;


namespace NeroWeNeed.Commons {
    [Serializable]
    public class SerializableReferenceDictionary<TKey, TValue> : ISerializationCallbackReceiver, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue> {
        private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
        [SerializeField]
        private List<TKey> keys = new List<TKey>();

        [SerializeReference]
        private List<TValue> values = new List<TValue>();

        public int Count { get => dictionary.Count; }


        public TValue this[TKey key]
        {
            get => dictionary[key];
            set => dictionary[key] = value;
        }

        public Dictionary<TKey, TValue> Value { get => dictionary; }

        public ICollection<TKey> Keys => throw new NotImplementedException();

        public ICollection<TValue> Values => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            return dictionary.GetEnumerator();
        }

        public void OnAfterDeserialize() {
            dictionary.Clear();
            for (int i = 0; i < keys.Count; i++) {
                dictionary[keys[i]] = values[i];
            }
        }

        public void OnBeforeSerialize() {
            keys.Clear();
            values.Clear();
            foreach (var kv in dictionary) {
                keys.Add(kv.Key);
                values.Add(kv.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return dictionary.GetEnumerator();
        }
        public void Add(TKey key, TValue value) {
            dictionary.Add(key, value);
        }

        public bool ContainsKey(TKey key) {
            return dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key) {
            return dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value) {
            return dictionary.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            dictionary.Add(item.Key, item.Value);
        }

        public void Clear() {
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            return dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {

            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            return dictionary.Remove(item.Key);
        }
    }
}

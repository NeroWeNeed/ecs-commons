using System;
using System.Collections;
using System.Collections.Generic;

namespace NeroWeNeed.Commons {
    public class DictionaryView<TOriginalKey, TOriginalValue, TKey, TValue> : IDictionary<TKey, TValue> {
        private Func<IDictionary<TOriginalKey, TOriginalValue>, IDictionary<TKey, TValue>> converter;
        private Func<IDictionary<TOriginalKey, TOriginalValue>> getter;
        private IDictionary<TKey, TValue> current;
        private int lastHash;
        public TValue this[TKey key]
        {
            get
            {
                RefreshIfDirty();
                return current[key];
            }
            set => throw new System.NotImplementedException();
        }

        public ICollection<TKey> Keys
        {
            get
            {
                RefreshIfDirty();
                return current.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                RefreshIfDirty();
                return current.Values;
            }
        }

        public int Count
        {
            get
            {
                RefreshIfDirty();
                return current.Count;
            }
        }

        public bool IsReadOnly => true;

        public DictionaryView(Func<IDictionary<TOriginalKey, TOriginalValue>> getter, Func<IDictionary<TOriginalKey, TOriginalValue>, IDictionary<TKey, TValue>> converter) {
            this.getter = getter;
            this.converter = converter;
            Refresh();

        }

        public void Add(TKey key, TValue value) {
            throw new System.NotImplementedException();
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            throw new System.NotImplementedException();
        }

        public void Clear() {
            throw new System.NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            RefreshIfDirty();
            return current.Contains(item);
        }
        private void RefreshIfDirty() {

            if (lastHash != (getter.Invoke()?.GetHashCode() ?? 0)) {
                Refresh();
            }
        }
        
        private void Refresh() {
            var value = getter.Invoke();
            if (value == null || value.Count == 0) {
                current = new Dictionary<TKey, TValue>();
                lastHash = 0;
            }
            else {
                current = converter.Invoke(value);
                lastHash = value?.GetHashCode() ?? 0;
            }

        }
        public bool ContainsKey(TKey key) {
            RefreshIfDirty();
            return current.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            throw new System.NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            RefreshIfDirty();
            return current.GetEnumerator();
        }

        public bool Remove(TKey key) {
            throw new System.NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            throw new System.NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TValue value) {
            RefreshIfDirty();
            return current.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            RefreshIfDirty();
            return current.GetEnumerator();
        }
    }
}
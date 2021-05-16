using System;
using System.Collections;
using System.Collections.Generic;

namespace NeroWeNeed.Commons {
    public class ListView<TOriginal, TNew> : IList<TNew> {
        private Func<IEnumerable<TOriginal>, IEnumerable<TNew>> converter;
        private Func<IEnumerable<TOriginal>> getter;

        private List<TNew> current = new List<TNew>();
        private int lastHash;
        public TNew this[int index]
        {
            get
            {
                RefreshIfDirty();
                return current[index];
            }
            set => throw new System.NotImplementedException();
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
        public ListView(Func<IEnumerable<TOriginal>> getter, Func<IEnumerable<TOriginal>, IEnumerable<TNew>> converter) {
            this.getter = getter;
            this.converter = converter;
            Refresh();
        }

        private void RefreshIfDirty() {
            if (lastHash != (getter.Invoke()?.GetHashCode() ?? 0)) {
                Refresh();
            }
        }

        private void Refresh() {
            var value = getter.Invoke();
            current.Clear();
            if (value != null) {
                current.AddRange(converter.Invoke(value));    
            }
            lastHash = value?.GetHashCode() ?? 0;
        }
        public void Add(TNew item) {
            throw new System.NotImplementedException();
        }

        public void Clear() {
            throw new System.NotImplementedException();
        }

        public bool Contains(TNew item) {
            RefreshIfDirty();
            return current.Contains(item);
        }

        public void CopyTo(TNew[] array, int arrayIndex) {
            throw new System.NotImplementedException();
        }

        public IEnumerator<TNew> GetEnumerator() {
            RefreshIfDirty();
            return current.GetEnumerator();
        }

        public int IndexOf(TNew item) {
            RefreshIfDirty();
            return current.IndexOf(item);
            
        }

        public void Insert(int index, TNew item) {
            throw new System.NotImplementedException();
        }

        public bool Remove(TNew item) {
            throw new System.NotImplementedException();
        }

        public void RemoveAt(int index) {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            RefreshIfDirty();
            return current.GetEnumerator();
        }
    }

}
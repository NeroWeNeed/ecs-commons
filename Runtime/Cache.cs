using System;
using System.Collections.Generic;

namespace NeroWeNeed.Commons {
    public interface ICache<TKey, TValue> : IDisposable {
        public TValue this[TKey key] { get; }
    }
    public class SimpleTypeCache<TValue> : ICache<Type, TValue> {
        private Dictionary<Type, TValue> cache = new Dictionary<Type, TValue>();
        public TValue this[Type key]
        {
            get
            {
                if (!cache.TryGetValue(key, out TValue value)) {
                    value = (TValue)Activator.CreateInstance(key);
                    cache[key] = value;
                }
                return value;
            }
        }

        public void Dispose() {
            cache.Clear();
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NeroWeNeed.Commons {
    public static class LinqExtensions {
        public static IEnumerable<TElement> NotNull<TElement>(this IEnumerable<TElement> elements) where TElement : class {
            return elements.Where(e => e != null);
        }
        public static IEnumerable<string> NotNullOrEmpty(this IEnumerable<string> elements) {
            return elements.Where(e => !string.IsNullOrEmpty(e));
        }
        public static IEnumerable<string> NotNullOrWhiteSpace(this IEnumerable<string> elements) {
            return elements.Where(e => !string.IsNullOrWhiteSpace(e));
        }
        
        public static IEnumerable<KeyValuePair<TKey, TElement>> ValueNotNull<TKey, TElement>(this IDictionary<TKey,TElement> elements) where TElement : class {
            return elements.Where(e => e.Value != null);
        }
        public static IEnumerable<TElement> NotDefault<TElement>(this IEnumerable<TElement> elements) {
            return elements.Where(e => !EqualityComparer<TElement>.Default.Equals(e, default));
        }
        public static bool IsEmpty(this ICollection collection) => collection.Count == 0;
        
    }

}
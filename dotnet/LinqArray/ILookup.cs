﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Ara3D
{
    /// <summary>
    /// Lookup table: mapping from a key to some value.
    /// </summary>
    public interface ILookup<TKey, TValue>
    {
        IArray<TKey> Keys { get; }
        IArray<TValue> Values { get; }
        bool Contains(TKey key);
        TValue this[TKey key] { get; }
    }

    public class EmptyLookup<TKey, TValue> : ILookup<TKey, TValue>
    {
        public IArray<TKey> Keys => LinqArray.Empty<TKey>();
        public IArray<TValue> Values => LinqArray.Empty<TValue>();
        public bool Contains(TKey key) => false;
        public TValue this[TKey key] => default;
    }

    public class LookupFromDictionary<TKey, TValue> : ILookup<TKey, TValue>
    {
        public IDictionary<TKey, TValue> Dictionary;

        public LookupFromDictionary(IDictionary<TKey, TValue> d = null)
        {
            Dictionary = d ?? new Dictionary<TKey, TValue>();
            // TODO: sort?
            Keys = d.Keys.ToIArray();
            Values = d.Values.ToIArray();
        }

        public IArray<TKey> Keys { get; }
        public IArray<TValue> Values { get; }
        public TValue this[TKey key] => Contains(key) ? Dictionary[key] : default;
        public bool Contains(TKey key) => Dictionary.ContainsKey(key);
    }

    public class LookupFromArray<TValue> : ILookup<int, TValue>
    {
        private IArray<TValue> array;

        public LookupFromArray(IArray<TValue> xs)
        {
            array = xs;
            Keys = array.Indices();
            Values = array;
        }

        public IArray<int> Keys { get; }
        public IArray<TValue> Values { get; }
        public TValue this[int key] => array[key];
        public bool Contains(int key) => key >= 0 && key <= array.Count;
    }

    public static class LookupExtensions
    {
        public static ILookup<TKey, TValue> ToLookup<TKey, TValue>(this IDictionary<TKey, TValue> d)
            => new LookupFromDictionary<TKey,TValue>(d);

        public static TValue GetOrDefault<TKey, TValue>(this ILookup<TKey, TValue> lookup, TKey key)
            => lookup.Contains(key) ? lookup[key] : default;

        public static IEnumerable<TValue> GetValues<TKey, TValue>(this ILookup<TKey, TValue> lookup)
            => lookup.Keys.ToEnumerable().Select(k => lookup[k]);
    }
}

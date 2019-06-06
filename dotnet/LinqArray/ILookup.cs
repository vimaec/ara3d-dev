using System;
using System.Collections.Generic;

namespace Ara3D
{
    /// <summary>
    /// Lookup table: mapping from a key to some value.
    /// </summary>
    public interface ILookup<TKey, TValue>
    {
        IArray<TKey> Keys { get; }
        IArray<TValue> Values { get; }
        TValue this[TKey key] { get; }
    }

    public class EmptyLookup<TKey, TValue> : ILookup<TKey, TValue>
    {
        public IArray<TKey> Keys => LinqArray.Empty<TKey>();
        public IArray<TValue> Values => LinqArray.Empty<TValue>();
        public TValue this[TKey key] => default;
    }

    public class LookupFromDictionary<TKey, TValue> : ILookup<TKey, TValue>
    {
        public IDictionary<TKey, TValue> Dictionary;

        public LookupFromDictionary(IDictionary<TKey, TValue> d)
        {
            Dictionary = d;
            // TODO: sort?
            Keys = d.Keys.ToIArray();
            Values = d.Values.ToIArray();
        }

        public IArray<TKey> Keys { get; }
        public IArray<TValue> Values { get; }
        public TValue this[TKey key] => Dictionary.ContainsKey(key) 
            ? Dictionary[key] : default;
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
    }

    public class LookupFromKeysAndFunction<TKey, TValue> : ILookup<TKey, TValue>
    {
        private Func<TKey, TValue> _func;

        public LookupFromKeysAndFunction(IArray<TKey> keys, Func<TKey, TValue> func)
        {
            _func = func;
            Keys = keys;
            Values = Keys.Select(func);
        }

        public IArray<TKey> Keys { get; }
        public IArray<TValue> Values { get; }
        public TValue this[TKey key] => _func(key);
    }

    public static class LookupExtensions
    {
        public static ILookup<TKey, TValue> ToLookup<TKey, TValue>(this IDictionary<TKey, TValue> d)
            => new LookupFromDictionary<TKey,TValue>(d);
    }

}

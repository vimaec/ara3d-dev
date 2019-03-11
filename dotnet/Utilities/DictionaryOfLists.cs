using System.Collections.Generic;
using System.Linq;

namespace Ara3D
{
    public class DictionaryOfLists<K, V> : Dictionary<K, List<V>>
    {
        public void Add(K k, V v)
        {
            if (!ContainsKey(k))
                Add(k, new List<V>());
            this[k].Add(v);
        }

        public IEnumerable<V> AllValues
            => Values.SelectMany(xs => xs);

        public List<V> GetOrDefault(K k) {
            if (!ContainsKey(k))
                return new List<V>();
            return this[k];
        }
    }
}

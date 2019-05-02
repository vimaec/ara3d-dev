using System.Collections.Generic;
using System.Linq;

namespace Ara3D
{
    public class IndexedSet<K> : Dictionary<K, int>
    {
        public int Add(K k)
        {
            if (!ContainsKey(k))
                Add(k, Count);
            return this[k];
        }

        public IEnumerable<K> OrderedKeys()
            => this.OrderBy(kv => kv.Value).Select(kv => kv.Key);
    }
}

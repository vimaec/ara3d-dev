using System.Collections.Generic;

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
    }
}

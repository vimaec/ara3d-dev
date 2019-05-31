using System.Collections.Generic;
using System.Linq;

namespace Ara3D
{
    public class DictionaryOfLists<TKey, TValue> : Dictionary<TKey, List<TValue>>
    {
        public DictionaryOfLists()
        { }

        public DictionaryOfLists(IEnumerable<IGrouping<TKey, TValue>> groups)
        {
            foreach (var grp in groups)
                Add(grp.Key, grp.ToList());
        }

        public void Add(TKey k, TValue v)
        {
            if (!ContainsKey(k))
                Add(k, new List<TValue>());
            this[k].Add(v);
        }

        public IEnumerable<TValue> AllValues
            => Values.SelectMany(xs => xs);

        public List<TValue> GetOrDefault(TKey k) {
            if (!ContainsKey(k))
                return new List<TValue>();
            return this[k];
        }
    }
}

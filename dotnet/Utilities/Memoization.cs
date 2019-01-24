using System;
using System.Collections.Concurrent;

namespace Ara3D
{
    public interface IMemoizer
    {
        ConcurrentDictionary<object, object> Cache { get; }
    }

    public static class Memoization
    {
        public static U Memoize<U>(this IMemoizer memo, Func<U> f) where U : class
        {
            return memo.Memoize(memo, x => f());
        }

        public static U Memoize<T, U>(this IMemoizer memo, T args, Func<T, U> f) where U: class
        {
            var key = Tuple.Create(args, f);
            if (memo.Cache.TryGetValue(key, out object r))
            {
                return r as U;
            }
            else
            {
                var obj = new object();
                lock (obj)
                {
                    return memo.Cache.GetOrAdd(key, f(args)) as U;
                }
            }
        }
    }
}

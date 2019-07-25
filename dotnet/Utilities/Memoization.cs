using System;

namespace Ara3D
{
    /// <summary>
    /// This class performs a simple memoization over one argument value. 
    /// </summary>
    public class MemoizedFunction<TInput, TOutput>
        where TInput : class
        where TOutput : class
    {
        TInput _input;
        TOutput _output;
        Func<TInput, TOutput> _compute;

        public MemoizedFunction(Func<TInput, TOutput> func)
            => _compute = func;

        /// <summary>
        /// When setting the input to a new value, the output is cleared. This indicated that the cache is dirty 
        /// </summary>
        public TInput Input
        {
            get { return _input; }
            set { if (_input != value) { _input = value; Reset(); } }
        }

        /// <summary>
        /// Forces a recompute the next time data is requested. 
        /// </summary>
        public void Reset()
        {
            _output = null;
        }

        /// <summary>
        /// Lazy evaluation of the output only when necessary. 
        /// </summary>
        public TOutput Output => _output ?? (_input != null ? _output = _compute(_input) : null);

        /// <summary>
        /// Treat this class as a function 
        /// </summary>
        public Func<TInput, TOutput> Call
            => (input) => { Input = input; return Output; };
    }

    public static class Memoizer
    {
        public static Func<TInput, TOutput> Memoize<TInput, TOutput>(this Func<TInput, TOutput> f)
            where TInput : class
            where TOutput : class
            => new MemoizedFunction<TInput, TOutput>(f).Call;
    }

}

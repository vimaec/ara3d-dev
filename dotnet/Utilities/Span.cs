using System;
using System.Runtime.InteropServices;

namespace Ara3D
{
    /// <summary>
    /// A span is a wrapper around a range of bytes in memory.
    /// Derived versions of this class implement IArray so that you
    /// can treat a pointer as an array. 
    /// </summary>
    public class BaseSpan
    {
        public readonly IByteSpan Bytes;
        public int Count { get; }

        public BaseSpan(IByteSpan bytes, int count)
        {                
            Bytes = bytes;
            Count = count;
        }
    }

    // This is probably going to be significantly slower than using one of the type specific spans
    public class Span<T> : BaseSpan, IArray<T>
    {
        public static int Size = typeof(T).SizeOf();

        public T this[int n] => Marshal.PtrToStructure<T>(Bytes.Ptr + Size * n);

        public Span(IByteSpan bytes)
            : base(bytes, bytes.ByteCount / Size)
        { }
    }
}

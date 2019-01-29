using System;

namespace Ara3D
{
    /// <summary>
    /// Points to an unmanaged contiguous sequence of bytes in memory. Useful for working with unsafe and unmanaged code.
    /// Very Similar to the concept of a Span/Memory in the more recent libraries, but much simpler. 
    /// </summary>
    public struct ByteSpan : IByteSpan
    {
        public int ByteCount { get; }
        public IntPtr Ptr { get; }

        public ByteSpan(IntPtr pointer, int size)
        {
            ByteCount = size;
            Ptr = pointer;
        }

        public ByteSpan(IntPtr begin, IntPtr end)
            : this(begin, (int)begin.Distance(end))
        {
        }
    }
}

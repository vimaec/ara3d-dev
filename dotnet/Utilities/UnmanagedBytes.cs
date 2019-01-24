using System;

namespace Ara3D
{
    /// <summary>
    /// Points to an unmanaged contiguous sequence of bytes in memory. Useful for working with unsafe and unmanaged code.
    /// Very Similar to the concept of a Span/Memory in the more recent libraries, but much simpler. 
    /// </summary>
    public struct UnmanagedBytes
    {
        public readonly int Size;
        public readonly IntPtr Ptr;

        public UnmanagedBytes(IntPtr pointer, int size)
        {
            Size = size;
            Ptr = pointer;
        }

        public UnmanagedBytes(IntPtr begin, IntPtr end)
            : this(begin, (int)begin.PtrDistance(end))
        {
        }
    }
}

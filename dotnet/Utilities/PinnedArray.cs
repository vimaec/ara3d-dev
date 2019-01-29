using System;
using System.Runtime.InteropServices;

namespace Ara3D
{
    /// <summary>
    /// Pins an array of Blittable structs so that we can access the data as bytes. Manages a GCHandle around the array.
    /// https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.marshal.unsafeaddrofpinnedarrayelement?view=netframework-4.7.2
    /// </summary>
    public sealed class PinnedArray<T> : IDisposable, IByteSpan
    {
        public GCHandle Handle { get; private set; }
        public T[] Array { get; private set; }
        public ByteSpan Bytes { get; private set; }

        // IByteSpan implementation
        public int ByteCount => Bytes.ByteCount;
        public IntPtr Ptr => Bytes.Ptr;

        public IntPtr ElementPointer(int n)
        {
#if DEBUG
            if (Bytes.Ptr == IntPtr.Zero)
                throw new Exception("Attempting to access unpinned array");
#endif
            return Marshal.UnsafeAddrOfPinnedArrayElement(Array, n);
        }

        public PinnedArray(T[] xs)
        {
            Array = xs;
            // This will fail if the underlying type is not Blittable (e.g. not contiguous in memory)
            Handle = GCHandle.Alloc(xs, GCHandleType.Pinned);
            Bytes = new ByteSpan(ElementPointer(0), ElementPointer(Array.Length)); 
        }

        void DisposeImplementation()
        {
            if (Bytes.Ptr != IntPtr.Zero)
            {
                Handle.Free();
                Bytes = new ByteSpan(IntPtr.Zero, 0);
            }
        }

        ~PinnedArray()
        {
            DisposeImplementation();
        }

        public void Dispose()
        {
            DisposeImplementation();
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Pins an array of elements so that we can access the data as bytes. Manages a GCHandle around the array.
    /// https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.marshal.unsafeaddrofpinnedarrayelement?view=netframework-4.7.2
    /// </summary>
    public sealed class PinnedArray : IDisposable, IByteSpan
    {
        public GCHandle Handle { get; private set; }
        public Array Array { get; private set; }
        public ByteSpan Bytes { get; private set; }

        // IByteSpan implementation
        public int ByteCount => Bytes.ByteCount;
        public IntPtr Ptr => Bytes.Ptr;        

        public PinnedArray(Array xs)
        {
            Array = xs;
            Handle = GCHandle.Alloc(xs, GCHandleType.Pinned);
            Bytes = Array.PinnedArrayToBytes();
        }

        void DisposeImplementation()
        {
            if (Bytes.Ptr != IntPtr.Zero)
            {
                Handle.Free();
                Bytes = new ByteSpan(IntPtr.Zero, 0);
            }
        }

        ~PinnedArray()
        {
            DisposeImplementation();
        }

        public void Dispose()
        {
            DisposeImplementation();
            GC.SuppressFinalize(this);
        }
    }
}

using System;
using System.Runtime.InteropServices;

namespace Ara3D
{
    /// <summary>
    /// Pins an array of Blittable structs so that we can access the data as bytes. Manages a GCHandle around the array.
    /// https://stackoverflow.com/questions/1318682/intptr-arithmetics/54462954#54462954
    /// </summary>
    public sealed class PinnedArray<T> : IDisposable, IBytes
    {
        public GCHandle Handle { get; }
        public T[] Array { get; }

        // IBytes implementation
        public int ByteCount { get; private set; }
        public IntPtr Ptr { get; private set; }
        
        public IntPtr ElementPointer(int n)
        {
            return Marshal.UnsafeAddrOfPinnedArrayElement(Array, n);
        }

        public PinnedArray(T[] xs)
        {
            Array = xs;
            // This will fail if the underlying type is not Blittable (e.g. not contiguous in memory)
            Handle = GCHandle.Alloc(xs, GCHandleType.Pinned);
            if (xs.Length != 0)
            {
                Ptr = ElementPointer(0);
                ByteCount = (int) Ptr.Distance(ElementPointer(Array.Length));
            }
            else
            {
                Ptr = IntPtr.Zero;
                ByteCount = 0;
            }
        }

        void DisposeImplementation()
        {
            if (Ptr != IntPtr.Zero)
            {
                Handle.Free();
                Ptr = IntPtr.Zero;
                ByteCount = 0;
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
    public sealed class PinnedArray : IDisposable, IBytes
    {
        public GCHandle Handle { get; }
        public Array Array { get; }

        // IBytes implementation
        public int ByteCount { get; private set; }
        public IntPtr Ptr { get; private set; }
    
        public PinnedArray(Array xs)
        {
            Array = xs;
            Handle = GCHandle.Alloc(xs, GCHandleType.Pinned);
            Ptr = ElementPointer(0);
            ByteCount = (int)Ptr.Distance(ElementPointer(Array.Length));
        }

        public IntPtr ElementPointer(int n)
        {
            return Marshal.UnsafeAddrOfPinnedArrayElement(Array, n);
        }

        void DisposeImplementation()
        {
            if (Ptr != IntPtr.Zero)
            {
                Handle.Free();
                ByteCount = 0;
                Ptr = IntPtr.Zero;
                ;
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

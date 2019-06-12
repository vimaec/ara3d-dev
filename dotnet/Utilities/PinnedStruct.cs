using System;
using System.Runtime.InteropServices;

namespace Ara3D
{
    /// <summary>
    /// Pins a struct so that we can access the data as bytes. Manages a GCHandle around the array.
    /// </summary>
    public class PinnedStruct<T> : IDisposable where T: struct 
    {
        public GCHandle Handle { get; }
        public T Value{ get; }
        public int ByteCount { get; private set; }
        public IntPtr Ptr { get; private set; }

        public PinnedStruct(T x)
        {
            Value = x;
            Handle = GCHandle.Alloc(Value, GCHandleType.Pinned);
            Ptr = Handle.AddrOfPinnedObject();
            ByteCount = typeof(T).SizeOf();
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

        ~PinnedStruct()
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
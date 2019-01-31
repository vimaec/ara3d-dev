using System;
using System.Runtime.InteropServices;

namespace Ara3D
{
    /// <summary>
    /// Pins a struct so that we can access the data as bytes. Manages a GCHandle around the array.
    /// </summary>
    public class PinnedStruct<T> : IDisposable, IBytes where T: struct 
    {
        public GCHandle Handle { get; }
        public T Value{ get; }
        public Bytes Bytes { get; private set; }

        // IBytes implementation
        public int ByteCount => Bytes.ByteCount;
        public IntPtr Ptr => Bytes.Ptr;

        public PinnedStruct(T x)
        {
            Value = x;
            Handle = GCHandle.Alloc(Value, GCHandleType.Pinned);
            Bytes = new Bytes(Handle.AddrOfPinnedObject(), typeof(T).SizeOf());
        }

        void DisposeImplementation()
        {
            if (Bytes.Ptr != IntPtr.Zero)
            {
                Handle.Free();
                Bytes = new Bytes(IntPtr.Zero, 0);
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
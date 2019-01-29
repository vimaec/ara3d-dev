using System;
using System.Runtime.InteropServices;

namespace Ara3D
{
    /// <summary>
    /// When you just want to allocate an array of bytes to share with unmanaged code.
    /// Alternatively I suppose one could use "SafeBuffer", but it is a complicated mess. 
    /// Note: SafeBuffer not "obsolete" as the docs say that is an error documented in the comments section. 
    /// TODO:
    /// * Check if we can switch to using the SafeBuffer under the hood.
    /// </summary>
    /// https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.marshal.allochglobal?view=netframework-4.7.2
    /// https://stackoverflow.com/questions/17562295/if-i-allocate-some-memory-with-allochglobal-do-i-have-to-free-it-with-freehglob/17563315#17563315
    /// https://docs.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.safebuffer?view=netframework-4.7.2
    /// https://github.com/Microsoft/referencesource/blob/master/mscorlib/system/runtime/interopservices/safebuffer.cs
    /// https://github.com/dotnet/corefx/blob/master/src/Common/src/CoreLib/System/Runtime/InteropServices/SafeBuffer.cs
    public sealed class UnmanagedBuffer : IDisposable, IByteSpan
    {
        public ByteSpan Bytes;

        // IByteSpan implementation
        public int ByteCount => Bytes.ByteCount;
        public IntPtr Ptr => Bytes.Ptr;

        public UnmanagedBuffer(int size)
        {
            // Honestly the advantages of "AllocCoTaskMem" versus "AllocHGlobal" (which maps to "AllocLocal" in the WIn API")
            // are not well documented. This is the recommended approach when we want to share data over COM calls, so 
            // I'm forced to assume it is slightly more general purpose. All is good as long as we use the appropriate
            // deallocator when disposing. 
            Bytes = new ByteSpan(Marshal.AllocCoTaskMem(size), size);
        }

        void DisposeImplementation()
        {            
            if (Bytes.Ptr != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(Bytes.Ptr);
                Bytes = new ByteSpan();
            }
        }

        ~UnmanagedBuffer()
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

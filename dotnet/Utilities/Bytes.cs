using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace Ara3D
{
    public interface IBytes 
    {
        int ByteCount { get; }
        IntPtr Ptr { get; }
    }

    public class IntPtrWrapper : IBytes
    {
        public int ByteCount { get; }
        public IntPtr Ptr { get; }

        public IntPtrWrapper(IntPtr ptr, int count)
        {
            Ptr = ptr;
            ByteCount = count;
        }
    }

    public unsafe class MemoryHandleWrapper : IBytes, IDisposable
    {
        public int ByteCount { get; }
        public IntPtr Ptr { get; }
        public MemoryHandle Handle { get; }

        public MemoryHandleWrapper(MemoryHandle h, int count)
        {
            Handle = h;
            ByteCount = count;
            Ptr = new IntPtr(h.Pointer);
        }

        public void Dispose()
        {
            Handle.Dispose();
        }
    }

    public static class BytesExtensions
    {
        public static bool Contains(this IBytes self, IntPtr ptr)
        {
            var dist = self.Ptr.Distance(ptr);
            return dist >= 0 && dist < self.ByteCount;
        }

        public static IntPtr End(this IBytes self)
        {
            return self.Ptr + self.ByteCount;
        }

        public static bool Overlapping(this IBytes self, IBytes other)
        {
            return self.Contains(other.Ptr) || self.Contains(other.End()) || other.Contains(self.Ptr) || self.Contains(self.End());
        }

        public static IBytes CopyTo(this IBytes self, IBytes other)
        {
            Util.MemoryCopy(self.Ptr, other.Ptr, Math.Min(self.ByteCount, other.ByteCount));
            return other;
        }

        public static byte[] CopyTo(this IBytes bytes, byte[] array)
        {
            Marshal.Copy(bytes.Ptr, array, 0, Math.Min(bytes.ByteCount, array.Length));
            return array;
        }

        public static byte[] CopyTo(this IBytes bytes, byte[] array, int offset, int size)
        {
            Marshal.Copy(bytes.Ptr, array, offset, Math.Min(bytes.ByteCount, size));
            return array;
        }

        public static IBytes CopyTo(this byte[] array, IBytes bytes)
        {
            Marshal.Copy(array, 0, bytes.Ptr, Math.Min(bytes.ByteCount, array.Length));
            return bytes;
        }

        public static byte[] ToBytes(this IBytes bytes)
        {
            return bytes.CopyTo(new byte[bytes.ByteCount]);
        }

        public static unsafe Span<T> ToSpan<T>(this IBytes span) where T: struct
        {
            return new Span<T>((void*)span.Ptr, span.ByteCount/ typeof(T).SizeOf());
        }

        public static T[] ToStructs<T>(this IBytes bytes) where T : struct
        {
            return bytes.ToSpan<T>().ToArray();
        }

        public static IBytes ToIBytes(this Memory<byte> bytes, int offset, int count)
        {
            return bytes.Slice(offset, count).ToIBytes();
        }

        public static IBytes ToIBytes(this Memory<byte> bytes)
        {
            return new MemoryHandleWrapper(bytes.Pin(), bytes.Length);
        }

        /// <summary>
        /// Returns true if the two buffers are the same.  
        /// </summary>
        public static bool SequenceEqual(this IBytes self, IBytes other)
        {
            // TODO: consider casting to a Vector<T>() https://docs.microsoft.com/en-us/dotnet/api/system.numerics.vector-1.-ctor?view=netcore-2.2#System_Numerics_Vector_1__ctor_System_Span__0__
            // Look at https://stackoverflow.com/questions/43289/comparing-two-byte-arrays-in-net/1445405#1445405
            // Compare to PInvoke
            if (self.ByteCount != other.ByteCount)
                return false;
            return self.ToSpan<byte>().SequenceEqual(other.ToSpan<byte>());
        }

        /*
        /// <summary>
        /// Copies the bytes to a buffer.
        /// </summary>
        public static byte[] CopyToBuffer(this IntPtrWrapper self, byte[] buffer)
        {
            if (buffer.Length < self.ByteCount)
                throw new Exception("Buffer is too small");
            Marshal.Copy(self.Ptr, buffer, 0, self.ByteCount);
            return buffer;
        }

        /// <summary>
        /// Copies the bytes to the buffer if it is big enough or allocates a new byte array if necessary.
        /// </summary>
        public static byte[] CopyToBufferOrAllocate(this IntPtrWrapper self, byte[] buffer)
        {
            if (buffer == null || buffer.Length < self.ByteCount)
                return self.ToByteArray();
            return self.CopyToBuffer(buffer);
        }
        */

    }
}

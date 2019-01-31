using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Ara3D
{
    public interface IBytes
    {
        int ByteCount { get; }
        IntPtr Ptr { get; }
    }

    public static class BytesExtensions
    {
        public static byte[] ToUtf8Bytes(this string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

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

        public static byte[] CopyTo(this IBytes span, byte[] array)
        {
            Marshal.Copy(span.Ptr, array, 0, Math.Min(span.ByteCount, array.Length));
            return array;
        }

        public static byte[] CopyTo(this IBytes span, byte[] array, int offset, int size)
        {
            Marshal.Copy(span.Ptr, array, offset, Math.Min(span.ByteCount, size));
            return array;
        }

        public static IBytes CopyTo(this byte[] array, IBytes span)
        {
            Marshal.Copy(array, 0, span.Ptr, Math.Min(span.ByteCount, array.Length));
            return span;
        }

        public static byte[] ToBytes(this IBytes span)
        {
            return span.CopyTo(new byte[span.ByteCount]);
        }

        public static unsafe Span<byte> ToSpan(this IBytes span)
        {
            return new Span<byte>((void*)span.Ptr, span.ByteCount);
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
            return self.ToSpan().SequenceEqual(other.ToSpan());
        }

        /*
        /// <summary>
        /// Copies the bytes to a buffer.
        /// </summary>
        public static byte[] CopyToBuffer(this Bytes self, byte[] buffer)
        {
            if (buffer.Length < self.ByteCount)
                throw new Exception("Buffer is too small");
            Marshal.Copy(self.Ptr, buffer, 0, self.ByteCount);
            return buffer;
        }

        /// <summary>
        /// Copies the bytes to the buffer if it is big enough or allocates a new byte array if necessary.
        /// </summary>
        public static byte[] CopyToBufferOrAllocate(this Bytes self, byte[] buffer)
        {
            if (buffer == null || buffer.Length < self.ByteCount)
                return self.ToByteArray();
            return self.CopyToBuffer(buffer);
        }
        */

    }
}

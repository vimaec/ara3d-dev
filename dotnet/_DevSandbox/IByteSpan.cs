using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Ara3D
{
    public interface IByteSpan
    {
        int ByteCount { get; }
        IntPtr Ptr { get; }
    }

    public static class ByteRangeExtensions
    {
        public static PinnedArray<byte> ToByteRange(this byte[] values)
        {
            return values.Pin();
        }

        public static IByteSpan SubRange(this IByteSpan self, int from, int count)
        {
            return new ByteSpan(self.Ptr + from, count);
        }

        public static IByteSpan ToByteRangeFromUtf8(this string s)
        {
            return Encoding.UTF8.GetBytes(s).ToByteRange();
        }

        public static bool Contains(this IByteSpan self, IntPtr ptr)
        {
            var dist = self.Ptr.Distance(ptr);
            return dist >= 0 && dist < self.ByteCount;
        }

        public static IntPtr End(this IByteSpan self)
        {
            return self.Ptr + self.ByteCount;
        }

        public static bool Overlapping(this IByteSpan self, IByteSpan other)
        {
            return self.Contains(other.Ptr) || self.Contains(other.End()) || other.Contains(self.Ptr) || self.Contains(self.End());
        }

        public static IByteSpan CopyTo(this IByteSpan self, IByteSpan other)
        {
            Util.MemoryCopy(self.Ptr, other.Ptr, Math.Min(self.ByteCount, other.ByteCount));
            return other;
        }

        public static byte[] CopyTo(this IByteSpan span, byte[] array)
        {
            Marshal.Copy(span.Ptr, array, 0, Math.Min(span.ByteCount, array.Length));
            return array;
        }

        public static byte[] CopyTo(this IByteSpan span, byte[] array, int offset, int size)
        {
            Marshal.Copy(span.Ptr, array, offset, Math.Min(span.ByteCount, size));
            return array;
        }

        public static IByteSpan CopyTo(this byte[] array, IByteSpan span)
        {
            Marshal.Copy(array, 0, span.Ptr, Math.Min(span.ByteCount, array.Length));
            return span;
        }

        public static byte[] ToBytes(this IByteSpan span)
        {
            return span.CopyTo(new byte[span.ByteCount]) as Byte[];
        }


    }
}

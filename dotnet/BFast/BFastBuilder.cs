using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Ara3D
{
    /// <summary>
    /// Constructs a BFast data structure in memory. 
    /// </summary>
    public static class BFastBuilder
    {
        public const int Alignment = 64;

        public static long ComputeNextAlignment(long n)
        {
            if (IsAligned(n))
                return n;
            return n + Alignment - (n % Alignment);
        }

        public static bool IsAligned(long n)
        {
            return n % Alignment == 0;
        }

        public static IntPtr CopyAndUpdatePtr<T>(this T value, IntPtr dest)
        {
            Marshal.StructureToPtr(value, dest, false);
            return dest + Marshal.SizeOf(value);
        }

        public static BFastBaseClass ToBFastBaseClass(this List<byte[]> buffers)
        {
            var r = new BFastBaseClass();

            // Construct the header
            r.Header.Magic = Constants.Magic;
            r.Header.NumArrays = buffers.Count;
            r.Header.DataStart = ComputeNextAlignment(r.Header.RangesEnd);
            // DataEnd is computed after iterating over all buffers

            // Compute the offsets for the data buffers
            var curIndex = r.Header.DataStart;
            for (var i = 0; i < buffers.Count; ++i)
            {
                Debug.Assert(IsAligned(curIndex));

                r.Ranges[i].Begin = curIndex;
                curIndex += buffers[i].Length;
                r.Ranges[i].End = curIndex;
                curIndex = ComputeNextAlignment(curIndex);

                Debug.Assert(r.Ranges[i].Count == buffers[i].Length);
            }

            // Finish with the header
            r.Header.DataEnd = curIndex;

            // Check that everything adds up 
            r.ValidateHeader();
            r.ValidateRanges();

            return r;
        }

        public static unsafe BFast ToBFastBytes(this List<byte[]> buffers)
        {
            var bfast = ToBFastBaseClass(buffers);

            // Allocate a data-buffer
            var data = new byte[bfast.Header.DataEnd];
            var pinnedData = data.Pin();

            // Copy the FileHeader to the bytes
            var ptr = pinnedData.Ptr;
            ptr = CopyAndUpdatePtr(bfast.Header, ptr);

            // Copy the ranges to the bytes
            for (var i = 0; i < bfast.Ranges.Length; ++i)
            {
                ptr = CopyAndUpdatePtr(bfast.Ranges[i], ptr);
            }

            // Copy the data-buffers
            fixed (byte* dest = &data[0])
            {
                for (var i = 0; i < buffers.Count; ++i)
                {
                    fixed (byte* src = &(buffers[i])[0])
                    {
                        var range = bfast.Ranges[i];
                        Buffer.MemoryCopy(src, dest + range.Begin, range.Count, range.Count);
                    }
                }
            }

            return new BFast(data);
        }
    }
}

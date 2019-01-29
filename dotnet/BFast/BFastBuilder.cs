using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        public static BFastBaseClass ToBFastBaseClass(this List<IByteSpan> buffers)
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
                curIndex += buffers[i].ByteCount;
                r.Ranges[i].End = curIndex;
                curIndex = ComputeNextAlignment(curIndex);

                Debug.Assert(r.Ranges[i].Count == buffers[i].ByteCount);
            }

            // Finish with the header
            r.Header.DataEnd = curIndex;

            // Check that everything adds up 
            r.ValidateHeader();
            r.ValidateRanges();

            return r;
        }

        public static BFastBytes ToBFastBytes(this List<IByteSpan> buffers)
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
            for (var i=0; i < buffers.Count; ++i)
            {
                buffers[i].CopyTo(data, (int)bfast.Ranges[i].Begin, (int)bfast.Ranges[i].Count);
            }

            return new BFastBytes(data);
        }
    }
}

/*
    BFAST - Binary Format for Array Streaming and Transmission
    Copyright 2018, Ara 3D, Inc.
    Usage licensed under terms of MIT License

    The BAST format is a simple, generic, and efficient representation of arrays of binary array data.     

    From a C# stand-point it is a method of concatenating an array of byte arrays in such 
    a way it can be read and loaded efficiently from disk, or over a network.  
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ara3D
{
    public static class BFastConstants
    {
        public const ushort Magic = 0xBFA5;
        public const ushort SameEndian = Magic;
        public const ushort SwappedEndian = 0x5AFB;
    }

    /// <summary>
    /// This tells us where a particular array begins and ends in relation to the beginning of a file.
    /// * Begin must be less than or equal to End.
    /// * Begin must be greater than or equal to DataStart
    /// * End must be less than or equal to DataEnd
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Pack = 4, Size = 16)]
    public struct Range
    {
        [FieldOffset(0)] public long Begin;
        [FieldOffset(8)] public long End;

        public long Count => End - Begin;
        public static int Size = 16;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4, Size = 32)]
    public struct FileHeader
    {
        [FieldOffset(0)] public long Magic;         // Either Constants.SameEndian or Constants.SwappedEndian depending on endianess of writer compared to reader. 
        [FieldOffset(8)] public long DataStart;     // <= file size and >= ArrayRangesEnd and >= FileHeader.ByteCount
        [FieldOffset(16)] public long DataEnd;       // >= DataStart and <= file size
        [FieldOffset(24)] public long NumArrays;     // number of arrays 

        /// <summary>
        /// This is where the array ranges are finished. 
        /// Must be less than or equal to DataStart.
        /// Must be greater than or equal to FileHeader.ByteCount
        /// </summary>
        public long RangesEnd => Size + NumArrays * 16;

        /// <summary>
        /// The size of the FileHeader structure 
        /// </summary>
        public static int Size = 32;

        /// <summary>
        /// Returns true if the producer of the BFast file has the same endianness as the current library
        /// </summary>
        public bool SameEndian => Magic == BFastConstants.SameEndian;
    };

    /// <summary>
    /// Wraps an array of byte buffers encoding a BFast structure and provides validation and safe access to the memory. 
    /// The BFAST file/data format is structured as follows:
    ///   * File header   - Fixed size file descriptor
    ///   * Ranges        - An array of pairs of offsets that point to the begin and end of each data arrays
    ///   * Array data    - All of the array data is contained in this section.
    /// </summary>
    public static class BFast
    {
        public const int Alignment = 32;

        public static long ComputeNextAlignment(long n)
            => IsAligned(n) ? n : n + Alignment - (n % Alignment);        

        public static long ComputePadding(long n)
            => ComputeNextAlignment(n) - n;        

        public static bool IsAligned(long n)
            => n % Alignment == 0;

        public static void WritePadding(BinaryWriter bw)
        {
            var padding = ComputePadding(bw.BaseStream.Position);
            for (var i = 0; i < padding; ++i)
                bw.Write((byte) 0);
        }

        public static FileHeader GetHeader(Memory<byte> bytes)
        {
            // Assure that the data is of sufficient size to get the header 
            if (FileHeader.Size > bytes.Length)
                throw new Exception($"Data length ({bytes.Length}) is smaller than size of FileHeader ({FileHeader.Size})");

            // Get the header
            var header = bytes.Slice(0, FileHeader.Size).Span.ToStruct<FileHeader>();
            ValidateHeader(header);
            return header;
        }

        public static Range[] GetRanges(FileHeader header, Memory<byte> bytes)
        {
            var ranges = bytes.Slice(FileHeader.Size, Range.Size * (int)header.NumArrays).Span.ToStructs<Range>();
            ValidateRanges(header, ranges);
            return ranges;
        }

        public static IList<Memory<byte>> LoadBFast(this byte[] bytes)
            => new Memory<byte>(bytes).LoadBFast();        

        public static IList<Memory<byte>> LoadBFast(this Memory<byte> bytes)
        {
            var header = GetHeader(bytes);
            var ranges = GetRanges(header, bytes);
            return ranges.Select(r => bytes.Slice((int)r.Begin, (int)r.Count)).ToList();
        }

        public static BinaryWriter Write(BinaryWriter bw, FileHeader header, Range[] ranges, IList<Memory<byte>> buffers)
        {
            bw.Write(header);
            WritePadding(bw);
            foreach (var r in ranges)
                bw.Write(r);
            WritePadding(bw);
            foreach (var b in buffers)
            {
                WritePadding(bw);
                bw.Write(b.ToBytes());
            }
            return bw;
        }

        public static byte[] ToBFastBytes(this IEnumerable<byte[]> buffers)
            => ToBFastBytes(buffers.Select(b => new Memory<byte>(b)));

        public static byte[] ToBFastBytes(this IEnumerable<Memory<byte>> buffers)
            => Write(new MemoryStream(), buffers.ToList()).ToArray();

        public static void ToBFastFile(this IEnumerable<Memory<byte>> buffers, string filePath)
            => Write(File.OpenWrite(filePath), buffers.ToList());

        public static T Write<T>(T stream, IList<Memory<byte>> buffers) where T: Stream
        {
            var header = new FileHeader();
            header.Magic = BFastConstants.Magic;
            header.NumArrays = buffers.Count;
            header.DataStart = ComputeNextAlignment(header.RangesEnd);

            // Allocate the data for the ranges
            var ranges = new Range[header.NumArrays];

            // Compute the offsets for the data buffers
            var curIndex = header.DataStart;
            for (var i = 0; i < buffers.Count; ++i)
            {
                Debug.Assert(IsAligned(curIndex));

                ranges[i].Begin = curIndex;
                curIndex += buffers[i].Length;
                ranges[i].End = curIndex;
                curIndex = ComputeNextAlignment(curIndex);

                Debug.Assert(ranges[i].Count == buffers[i].Length);
            }

            // Finish with the header
            header.DataEnd = curIndex;

            // Check that everything adds up 
            ValidateHeader(header);
            ValidateRanges(header, ranges);

            // Write the data 
            using (var bw = new BinaryWriter(stream))
                Write(bw, header, ranges, buffers);

            return stream;
        }

        /// <summary>
        /// Checks that the header values are sensible, and throws an exception otherwise.
        /// </summary>
        public static void ValidateHeader(FileHeader header)
        {
            if (header.Magic != BFastConstants.SameEndian && header.Magic != BFastConstants.SwappedEndian)
                throw new Exception($"Invalid magic number {header.Magic}");

            if (header.DataStart < FileHeader.Size)
                throw new Exception($"Data start {header.DataStart} cannot be before the file header size {FileHeader.Size}");

            if (header.DataStart > header.DataEnd)
                throw new Exception($"Data start {header.DataStart} cannot be after the data end {header.DataEnd}");

            if (header.NumArrays < 0)
                throw new Exception($"Number of arrays {header.NumArrays} is not a positive number");

            if (header.RangesEnd > header.DataStart)
                throw new Exception($"Computed arrays ranges end must be less than the start of data {header.DataStart}");
        }

        /// <summary>
        /// Checks that the range values are sensible, and throws an exception otherwise.
        /// </summary>
        public static void ValidateRanges(FileHeader header, Range[] ranges)
        {
            if (ranges == null)
                throw new Exception("Ranges must not be null");

            var min = header.DataStart;
            var max = header.DataEnd;

            for (var i = 0; i < ranges.Length; ++i)
            {
                var begin = ranges[i].Begin;
                var end = ranges[i].End;
                if (begin < min || begin > max)
                    throw new Exception($"Array offset begin {begin} is not in valid span of {min} to {max}");
                if (i > 0)
                    if (begin < ranges[i - 1].End)
                        throw new Exception($"Array offset begin {begin} is overlapping with previous array {ranges[i - 1].End}");
                if (end < begin || end > max)
                    throw new Exception($"Array offset end {end} is not in valid span of {begin} to {max}");
            }
        }
    }
}

/*
    BFAST - Binary Format for Array Streaming and Transmission
    Copyright 2018, Ara 3D, Inc.
    Usage licensed under terms of MIT License

    The BFAST format is a simple, generic, and efficient representation of arrays of binary data (bytes).
    It can be used in place of a zip when compression is not required, or when a simple protocol
    is required for transmitting data to/from disk, between processes, or over a network. 

    The BFAST is designed to detect when the endianness of the producer and the consumer application
    is different. Each BFAST array is aligned on 32-byte boundaries. The beginning and end of each 
    data buffer is stored in a table near the beginning of a BFAST file. 

    The first buffer in a BFAST is read as a sequence of Utf-8 encoded strings separated by null '\0' 
    characters each representing the name of each buffer.    

    The BFAST has the following structure:
        * header - BFAST magic number, beginning and end index of data block (in bytes), and number of arrays  
        * ranges - An array of structs each containing the beginning and end indcex of the data block. 
        * data - The raw data-block, containing the concatenated (and aligned) data for all buffers. 
        
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
        public const long Magic = 0xBFA5;
        public const long SameEndian = Magic;
        public const long SwappedEndian = 0x5AFB << 16;
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
        public const int ALIGNMENT = 32;

        /// <summary>
        /// Given a position in the stream, tells us where the the next aligned position will be, if it the current position is not aligned.
        /// </summary>
        public static long ComputeNextAlignment(long n)
            => IsAligned(n) ? n : n + ALIGNMENT - (n % ALIGNMENT);

        /// <summary>
        /// Given a position in the stream, computes how much padding is required to bring the value to an algitned point. 
        /// </summary>
        public static long ComputePadding(long n)
            => ComputeNextAlignment(n) - n;

        /// <summary>
        /// Given a position in the stream, tells us whether the position is aligned.
        /// </summary>
        public static bool IsAligned(long n)
            => n % ALIGNMENT == 0;

        /// <summary>
        /// Write enough padding bytes to bring the current stream position to an aligned position
        /// </summary>
        public static void WritePadding(BinaryWriter bw)
        {
            var padding = ComputePadding(bw.BaseStream.Position);
            for (var i = 0; i < padding; ++i)
                bw.Write((byte)0);
            Debug.Assert(IsAligned(bw.BaseStream.Position));
        }

        /// <summary>
        /// Extracts the BFast header from the first bytes of a span.
        /// </summary>
        public static FileHeader GetHeader(Span<byte> bytes)
        {
            // Assure that the data is of sufficient size to get the header 
            if (FileHeader.Size > bytes.Length)
                throw new Exception($"Data length ({bytes.Length}) is smaller than size of FileHeader ({FileHeader.Size})");

            // Get the values that make up the header
            var values = MemoryMarshal.Cast<byte, long>(bytes).Slice(0, 4).ToArray();
            var header = new FileHeader
            {
                Magic = values[0],
                DataStart = values[1],
                DataEnd = values[2],
                NumArrays = values[3],
            };
            ValidateHeader(header);
            return header;
        }

        /// <summary>
        /// Extracts the data range structs from a byte span given the file header. Also performs basic validation.
        /// </summary>
        public static Range[] GetRanges(FileHeader header, Span<byte> bytes)
        {
            var rangeByteSpan = bytes.Slice(FileHeader.Size, Range.Size * (int)header.NumArrays);
            var rangeSpan = MemoryMarshal.Cast<byte, Range>(rangeByteSpan);
            var ranges = rangeSpan.ToArray();
            ValidateRanges(header, ranges);
            return ranges;
        }

        /// <summary>
        /// Given an array of bytes representing a BFast file, returns the array of data buffers. 
        /// </summary>
        public static IList<IBuffer> ToBFastRawBuffers(this byte[] bytes)
            => new Memory<byte>(bytes).ToBFastRawBuffers();

        /// <summary>
        /// Given a memory block of bytes representing a BFast file, returns the array of data buffers,
        /// </summary>
        public static IList<IBuffer> ToBFastRawBuffers(this Memory<byte> bytes)
        {
            var header = GetHeader(bytes.Span);
            var ranges = GetRanges(header, bytes.Span);
            return ranges.Select(r => bytes.Slice((int)r.Begin, (int)r.Count).ToBuffer()).ToList();
        }

        /// <summary>
        /// Loads a BFast file from the given path 
        /// </summary>
        public static IList<IBuffer> ReadRawBFast(string filePath)
            => ToBFastRawBuffers(File.ReadAllBytes(filePath));

        /// <summary>
        /// Helper function that converts an array of structs into an array of bytes using the MemoryMarshal class
        /// </summary>
        public static byte[] ToBytes<T>(T[] xs) where T: struct
            => MemoryMarshal.Cast<T, byte>(xs).ToArray();

        /// <summary>
        /// Helper function that converts a struct into an array of bytes using the MemoryMarshal class
        /// </summary>
        public static byte[] ToBytes<T>(T x) where T : struct
            => ToBytes(new[] { x });

        /// <summary>
        /// Writes a BFast using the provided BinaryWriter
        /// </summary>
        public static BinaryWriter Write(BinaryWriter bw, FileHeader header, Range[] ranges, IList<Memory<byte>> buffers)
        {
            bw.Write(header.Magic);
            bw.Write(header.DataStart);
            bw.Write(header.DataEnd);
            bw.Write(header.NumArrays);
            foreach (var r in ranges)
                bw.Write(ToBytes(r));
            WritePadding(bw);
            foreach (var b in buffers)
            {
                WritePadding(bw);
                bw.Write(b.ToArray());
            }
            return bw;
        }

        /// <summary>
        /// Converts an array of byte arrays to a BFAST file format in memory. 
        /// </summary>
        public static byte[] ToBFastBytes(this IEnumerable<IBuffer> buffers)
            => ToBFastBytes(buffers.Select(b => new Memory<byte>(b)));

        /// <summary>
        /// Converts an array of data buffers to a BFAST file format in memory. 
        /// </summary>
        public static byte[] ToBFastBytes(this IEnumerable<IBuffer> buffers)
            => Write(buffers.ToList(), new MemoryStream()).ToArray();

        /// <summary>
        /// Writes an array of data buffers to the given file. 
        /// </summary>
        public static void ToBFastFile(this IEnumerable<IBuffer> buffers, string filePath)
            => Write(buffers, File.OpenWrite(filePath));

        /// <summary>
        /// Writes an array of data buffers to the given file. 
        /// </summary>
        public static void ToBFastFile(this IEnumerable<byte[]> buffers, string filePath)
            => Write(buffers.Select(buffer => buffer.AsMemory()), File.OpenWrite(filePath));

        /// <summary>
        /// Writes an array of data buffers to the given data stream 
        /// </summary>
        public static T Write<T>(this IEnumerable<IBuffer> enumBuffers, T stream) where T: Stream
        {
            var buffers = enumBuffers.ToList();

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

        public static IEnumerable<Tuple<string, Memory<byte>>> 
    }
}

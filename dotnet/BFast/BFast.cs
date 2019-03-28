/*
    BFAST - Binary Format for Array Streaming and Transmission
    Copyright 2018, Ara 3D, Inc.
    Usage licensed under terms of MIT License

    The BAST format is a simple, generic, and efficient representation of arrays of binary array data.     
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

// The BFast is a data format for arrays of binary data. 
namespace Ara3D
{
    /// <summary>
    /// The programmatic interface of a BFast data structure.
    /// </summary>
    public interface IBFast
    {
        FileHeader Header { get; }
        Range[] Ranges { get; }
        IBytes GetBuffer(int n);
    }

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
    /// This is the base class of various BFast implementations, whether they are MemoryMappedFiles or in memory implementations.
    /// </summary>
    public class BFastBaseClass
    {
        public FileHeader _header;
        public Range[] _ranges;

        public FileHeader Header => _header;
        public Range[] Ranges => _ranges;

        public void ValidateHeader()
        {
            // Check each value in the header
            if (Header.Magic != BFastConstants.SameEndian && Header.Magic != BFastConstants.SwappedEndian)
                throw new Exception($"Invalid magic number {Header.Magic}");

            if (Header.DataStart < FileHeader.Size)
                throw new Exception($"Data start {Header.DataStart} cannot be before the file header size {FileHeader.Size}");

            if (Header.DataStart > Header.DataEnd)
                throw new Exception($"Data start {Header.DataStart} cannot be after the data end {Header.DataEnd}");

            if (Header.NumArrays < 0)
                throw new Exception($"Number of arrays {Header.NumArrays} is not a positive number");

            if (Header.RangesEnd > Header.DataStart)
                throw new Exception($"Computed arrays ranges end must be less than the start of data {Header.DataStart}");
        }

        public void ValidateRanges()
        {
            if (Ranges == null)
                throw new Exception("Ranges must not be null");

            var min = Header.DataStart;
            var max = Header.DataEnd;

            for (var i = 0; i < _ranges.Length; ++i)
            {
                var begin = _ranges[i].Begin;
                var end = _ranges[i].End;
                if (begin < min || begin > max)
                    throw new Exception($"Array offset begin {begin} is not in valid span of {min} to {max}");
                if (i > 0)
                    if (begin < _ranges[i - 1].End)
                        throw new Exception($"Array offset begin {begin} is overlapping with previous array {_ranges[i - 1].End}");
                if (end < begin || end > max)
                    throw new Exception($"Array offset end {end} is not in valid span of {begin} to {max}");
            }
        }
    }

    /// <summary>
    /// Wraps an array of byte buffers encoding a BFast structure and provides validation and safe access to the memory. 
    /// The BFAST file/data format is structured as follows:
    ///   * File header   - Fixed size file descriptor
    ///   * Ranges        - An array of pairs of offsets that point to the begin and end of each data arrays
    ///   * Array data    - All of the array data is contained in this section.
    /// </summary>
    public class BFast : BFastBaseClass, IBFast
    {
        public readonly IList<IBytes> Buffers;

        /// <summary>
        /// Creates a BFast structure from the data in memory. 
        /// </summary>
        public BFast(byte[] data)
            : this(new Memory<byte>(data))
        { }

        public const int Alignment = 32;

        public static long ComputeNextAlignment(long n)
        {
            if (IsAligned(n))
                return n;
            return n + Alignment - (n % Alignment);
        }

        public static long ComputePadding(long n)
        {
            return ComputeNextAlignment(n) - n;
        }

        public static bool IsAligned(long n)
        {
            return n % Alignment == 0;
        }

        public BFast(IList<IBytes> buffers)
        {
            Buffers = buffers;

            // Construct the header
            _header.Magic = BFastConstants.Magic;
            _header.NumArrays = buffers.Count;
            _header.DataStart = ComputeNextAlignment(Header.RangesEnd);
            // DataEnd is computed after iterating over all buffers

            // Allocate the data for the ranges
            _ranges = new Range[_header.NumArrays];

            // Compute the offsets for the data buffers
            var curIndex = Header.DataStart;
            for (var i = 0; i < buffers.Count; ++i)
            {
                Debug.Assert(IsAligned(curIndex));

                _ranges[i].Begin = curIndex;
                curIndex += buffers[i].ByteCount;
                _ranges[i].End = curIndex;
                curIndex = ComputeNextAlignment(curIndex);

                Debug.Assert(_ranges[i].Count == buffers[i].ByteCount);
            }

            // Finish with the header
            _header.DataEnd = curIndex;

            // Check that everything adds up 
            ValidateHeader();
            ValidateRanges();
        }

        /// <summary>
        /// Creates a BFast structure from the data in memory.
        /// This constructor assumed no more than 2GB of memory.
        /// </summary>
        public BFast(Memory<byte> memory)
        {
            // Assure that the data is of sufficient size to get the header 
            if (FileHeader.Size > memory.Length)
                throw new Exception($"Data length ({memory.Length}) is smaller than size of FileHeader ({FileHeader.Size})");

            // Get the header
            _header = memory.Slice(0, (int) FileHeader.Size).Span.ToStruct<FileHeader>();
            ValidateHeader();

            // Get the ranges 
            _ranges = memory.Slice(FileHeader.Size, Range.Size * (int) Header.NumArrays).Span.ToStructs<Range>();
            ValidateRanges();

            // Get the buffers
            Buffers = _ranges.Select(r => memory.ToIBytes((int)r.Begin, (int)r.Count)).ToList();
        }

        public IBytes GetBuffer(int n)
            => Buffers[n];
    }

    public static class BFastExtensions
    {
        public static int NumBuffers(this IBFast bf) => (int)bf.Header.NumArrays;

        public static IEnumerable<IBytes> GetBuffers(this IBFast bf)
        {
            for (var i = 0; i < bf.NumBuffers(); ++i)
                yield return bf.GetBuffer(i);
        }

        public static BFast AsBFast(this IEnumerable<byte[]> buffers)
            => new BFast(buffers.Select(b => b.Pin() as IBytes).ToList());

        public static void WritePadding(BinaryWriter bw)
        {
            var padding = BFast.ComputePadding(bw.BaseStream.Position);
            for (var i = 0; i < padding; ++i)
                bw.Write((byte) 0);
        }

        public static Stream Write(this IBFast bf, Stream stream)
        {
            using (var bw = new BinaryWriter(stream))
                bf.Write(bw);
            return stream;
        }

        public static BinaryWriter Write(this IBFast bf, BinaryWriter bw)
        {
            bw.Write(bf.Header);
            WritePadding(bw);
            bw.Write(bf.Ranges);
            WritePadding(bw);
            foreach (var b in bf.GetBuffers())
            {
                bw.Write(b);
                WritePadding(bw);
            }

            return bw;
        }

        public static void WriteToFile(this IBFast bf, string path)
        {
            using (var f = File.OpenWrite(path))
                bf.Write(f);
        }

        public static byte[] ToBytes(this IBFast bf)
        {
            using (var mem = new MemoryStream()) {
                bf.Write(mem);
                return mem.ToArray();
            }
        }

        public static BFast AsBFast(this byte[] bytes)
            => new BFast(bytes);

        public static BFast ReadBFast(string file)
            => File.ReadAllBytes(file).AsBFast();

        public static void ReadBFast(Stream stream)
            => stream.ReadAllBytes().AsBFast();
    }
    /*

    /// <summary>
    /// Opens a BFAST file as a memory mapped file. This enables you to read the header, 
    /// the locations of the data and only the specific arrays that you are interested in. 
    /// </summary>
    public class BFastFileReader : BFastBaseClass, IDisposable
    {
        public MemoryMappedFile MappedFile;

        public BFastFileReader(string filePath)
        {
            Open(filePath);

            using (var accessor = MappedFile.CreateViewAccessor(0, FileHeader.Size))
                accessor.Read(0, out _header);

            ValidateHeader();

            _ranges = new Range[Header.NumArrays];

            using (var accessor = MappedFile.CreateViewAccessor(FileHeader.Size, Range.Size * Count))
                for (var i = 0; i < Count; ++i)
                    accessor.Read(i * Range.Size, out _ranges[i]);

            ValidateRanges();
        }

        /// <summary>
        /// Returns the number of arrays. 
        /// </summary>
        public int Count => _ranges.Length;

        public Memory<byte> GetBuffer(int n)
        {
            // Will throw an overflow exception if the buffer is longer than 2GB. 
            return MappedFile.ReadBytes(_ranges[n].Begin, (int)_ranges[n].Count).ToMemory();
        }

        public void Open(string filePath)
        {
            // https://docs.microsoft.com/en-us/dotnet/api/system.io.memorymappedfiles.memorymappedfile.createfromfile?view=netframework-4.7.2
            MappedFile = MemoryMappedFile.CreateFromFile(filePath);
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (MappedFile != null)
            {
                MappedFile.Dispose();
                MappedFile = null;
            }
        }
    }
    */
}

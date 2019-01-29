using System;
using System.Runtime.InteropServices;

namespace Ara3D
{
    public static class Constants
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

        public long Count { get { return End - Begin; } }
        public static long Size = 16;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 4, Size = 32)]
    public struct FileHeader
    {
        [FieldOffset(0)]    public long Magic;         // Either Constants.SameEndian or Constants.SwappedEndian depending on endianess of writer compared to reader. 
        [FieldOffset(8)]    public long DataStart;     // <= file size and >= ArrayRangesEnd and >= FileHeader.ByteCount
        [FieldOffset(16)]   public long DataEnd;       // >= DataStart and <= file size
        [FieldOffset(24)]   public long NumArrays;     // number of arrays 

        /// <summary>
        /// This is where the array ranges are finished. 
        /// Must be less than or equal to DataStart.
        /// Must be greater than or equal to FileHeader.ByteCount
        /// </summary>
        public long RangesEnd { get { return Size + NumArrays * 16; } }

        /// <summary>
        /// The size of the FileHeader structure 
        /// </summary>
        public static long Size = 32;

        /// <summary>
        /// Where a particular span can be found relative to the beginning of a file. 
        /// </summary>
        public long GetOffsetOfRange(long n) {
            if (n < 0 || n >= NumArrays)
                throw new IndexOutOfRangeException();
            return Size + Range.Size * n;
        }
    };
}

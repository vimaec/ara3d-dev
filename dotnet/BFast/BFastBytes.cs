using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ara3D
{
    /// <summary>
    /// Wraps an array of bytes encoding a BFast structure and provides validation and safe access to the memory. 
    /// The BFAST file/data format is structured as follows:
    ///   * File header   - Fixed size file descriptor
    ///   * Ranges        - An array of pairs of offsets that point to the begin and end of each data arrays
    ///   * Array data    - All of the array data is contained in this section.
    /// </summary>
    public class BFastBytes : BFastBaseClass
    {
        public long RawDataLength => RawData.LongLength;
        public byte[] RawData { get; }
        public bool SameEndianness => Header.Magic == Constants.SameEndian;
        public long Count => Header.NumArrays;

        /// <summary>
        /// Creates a BFast structure from the data in memory. 
        /// </summary>
        /// <param name="data"></param>
        public BFastBytes(byte[] data)
        {
            RawData = data;

            // Assure that the data is of sufficient size to get the header 
            if (FileHeader.Size > RawDataLength)
                throw new Exception($"Data length ({data.Length}) is smaller than size of FileHeader ({FileHeader.Size})");

            // Get the header
            Header = MarshalHeader();
            ValidateHeader();

            Ranges = MarshalRanges((int)Header.NumArrays);
            ValidateRanges();
         }

        public static BFastBytes ReadFile(string file)
        {
            return new BFastBytes(File.ReadAllBytes(file));
        }

        public byte[] GetBuffer(int n)
        { 
            {
                var r = new byte[Ranges[n].Count];
                Array.Copy(RawData, Ranges[n].Begin, r, 0, r.Length);
                return r;
            }
        }

        FileHeader MarshalHeader()
        {
            var intPtr = Marshal.AllocHGlobal((int)FileHeader.Size);
            try
            {
                Marshal.Copy(RawData, 0, intPtr, (int)FileHeader.Size);
                return Marshal.PtrToStructure<FileHeader>(intPtr);
            }
            finally
            {
                Marshal.FreeHGlobal(intPtr);
            }
        }
        
        Range[] MarshalRanges(int n)
        {
            var ranges = new Range[n];
            var intPtr = Marshal.AllocHGlobal((int)Range.Size);
            try
            {
                for (var i = 0; i < n; ++i)
                {
                    var offset = Header.GetOffsetOfRange(i);
                    Marshal.Copy(RawData, (int)Header.GetOffsetOfRange(i), intPtr, (int)Range.Size);
                    ranges[i] = Marshal.PtrToStructure<Range>(intPtr);
                }
                return ranges;
            }
            finally
            {
                Marshal.FreeHGlobal(intPtr);
            }
        }

        public void WriteToFile(string path)
        {
            File.WriteAllBytes(path, RawData);
        }
    }
}

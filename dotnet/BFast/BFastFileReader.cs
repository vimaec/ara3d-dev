using System;
using System.IO.MemoryMappedFiles;

namespace Ara3D
{
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
                accessor.Read(0, out Header);

            ValidateHeader();

            var ranges = new Range[Header.NumArrays];

            using (var accessor = MappedFile.CreateViewAccessor(Header.GetOffsetOfRange(0), Range.Size * Count))
                for (var i = 0; i < Count; ++i)
                    accessor.Read(i * Range.Size, out Ranges[i]);

            ValidateRanges();
        }

        /// <summary>
        /// Returns the number of arrays. 
        /// </summary>
        public int Count => Ranges.Length;

        public byte[] GetRange(int n)
        {
            // Will throw an overflow exception if the buffer is longer than 2GB. 
            return MappedFile.ReadBytes(Ranges[n].Begin, (int)Ranges[n].Count);
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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ara3D
{
    /// <summary>
    /// This is the base class of various BFast implementations, whether they are MemoryMappedFiles or in memory implementations.
    /// </summary>
    public class BFastBaseClass 
    {
        public FileHeader Header;
        public Range[] Ranges;

        public void ValidateHeader()
        {
            // Check each value tin the header
            if (Header.Magic != Constants.SameEndian && Header.Magic != Constants.SwappedEndian)
                throw new Exception($"Invalid magic number {Header.Magic}");

            if (Header.DataStart < FileHeader.Size)
                throw new Exception($"Data start {Header.DataStart} cannot be before the file header size {FileHeader.Size}");

            if (Header.DataStart > Header.DataEnd)
                throw new Exception($"Data start {Header.DataStart} cannot be after the data end {Header.DataEnd}");

            if (Header.NumArrays < 0)
                throw new Exception($"Number of arrays {Header.NumArrays} is not a positive number");

            if (Header.RangesEnd < Header.DataStart)
                throw new Exception($"Computed arrays ranges end must be less than the start of data {Header.DataStart}");
        }

        public void ValidateRanges()
        {
            var min = Header.DataStart;
            var max = Header.DataEnd;

            for (var i = 0; i < Ranges.Length; ++i)
            {
                var begin = Ranges[i].Begin;
                var end = Ranges[i].End;
                if (begin < min || begin > max)
                    throw new Exception($"Array offset begin {begin} is not in valid span of {min} to {max}");
                if (i > 0)
                    if (begin < Ranges[i - 1].End)
                        throw new Exception($"Array offset begin {begin} is overlapping with previous array {Ranges[i - 1].End}");
                if (end < begin || end > max)
                    throw new Exception($"Array offset end {end} is not in valid span of {begin} to {max}");
            }
        }
    }
}

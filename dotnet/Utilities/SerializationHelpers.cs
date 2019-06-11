using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Ara3D
{
    /// <summary>
    /// These are tools for converting from arrays of structs (including primitives) 
    /// to and from bytes. This is useful for writing custom high-performance serializers. 
    /// </summary>
    public static unsafe class SerializationHelpers
    {
        /// <summary>
        /// Converts an array of bytes to an array of structs (including primitives)
        /// </summary>
        public static T[] FromBytes<T>(this byte[] self) where T : struct
        {
            var structSize = Marshal.SizeOf(typeof(T));
            Debug.Assert(self.Length % structSize == 0);
            var count = self.Length / structSize;
            var r = new T[count];

            if (typeof(T).IsPrimitive)
            {
                Buffer.BlockCopy(self, 0, r, 0, self.Length);
            }
            else
            {
                fixed (byte* p = self)
                {
                    var ip = new IntPtr(p);
                    for (var i = 0; i < count; ++i)
                    {
                        r[i] = Marshal.PtrToStructure<T>(ip);
                        ip += structSize;
                    }
                }
            }

            return r;
        }

        /// <summary>
        /// Writes an array of structs (or primitives) to a stream, preceded by the number of bytes
        /// </summary>
        public static void WriteStructs<T>(this BinaryWriter self, T[] xs) where T : struct
        {
            var buffer = xs.ToBytes();
            self.Write((long)buffer.Length);
            self.Write(buffer);
        }

        /// <summary>
        /// Reads an array of structs (or primitives) from a stream, where the number of bytes is first given.
        /// </summary>
        public static T[] ReadStructs<T>(this BinaryReader self) where T: struct
        {
            var structSize = Marshal.SizeOf<T>();
            var length = self.ReadInt64();
            if (length % structSize != 0)
                throw new Exception("Invalid deserialization format");
            if (length > int.MaxValue)
                throw new Exception($"Cannot allocate buffer larger than {int.MaxValue}");
            var tmp = self.ReadBytes((int)length);
            return tmp.FromBytes<T>();
        }
    }
}

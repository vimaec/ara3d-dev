using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Ara3D
{
    /// <summary>
    /// Classes that support this interface can be easily read and written efficiently to file
    /// </summary>
    public interface IBinarySerializable
    {
        void Write(BinaryWriter bw);
        void Read(BinaryReader br);
    }

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
        /// Writes an array of classes to a stream where the classes sup
        /// </summary>
        public static void WriteClasses<T>(this BinaryWriter self, ICollection<T> values) where T : IBinarySerializable
        {
            self.Write((long)values.Count);
            foreach (var v in values)
                v.Write(self);
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

        /// <summary>
        /// Deserializes an array of classes that support the interface "IBinarySerializable"
        /// </summary>
        public static T[] ReadClasses<T>(this BinaryReader self) where T: IBinarySerializable, new()
        {
            var length = self.ReadInt64();
            var r = new T[length];
            for (var i=0; i < length; ++i)
            {
                r[i] = new T();
                r[i].Read(self);
            }
            return r;
        }

        /// <summary>
        /// Reads the data struct from the given file.
        /// </summary>
        public static T Read<T>(this T self, string file) where T: IBinarySerializable
        {
            using (var fs = File.OpenRead(file))
            using (var br = new BinaryReader(fs))
            {
                self.Read(br);
                return self;
            }
        }

        /// <summary>
        /// Writes the data structure to the file.
        /// </summary>
        public static void Write<T>(this T self, string file) where T: IBinarySerializable
        {
            using (var fs = File.OpenWrite(file))
            using (var bw = new BinaryWriter(fs))
            {
                self.Write(bw);
            }
        }

        // TODO: This can allow us to throw out "IBinarySerializable"
        // T is either a primitive, struct, class, or collection of classes or structs, or unknown 
        public static Func<T, BinaryWriter> GetWriterFunction<T>()
        {
            throw new NotImplementedException();
        }

        // T is either a primitive, struct, class, or collection of classes or structs, or unknown 
        public static Func<BinaryReader, T> GetReaderFunc<T>()
        {
            throw new NotImplementedException();
        }
    }
}

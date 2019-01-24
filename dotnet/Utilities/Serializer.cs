using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Ara3D;

namespace Ara3D
{
    public static class FastSerializer
    {
        public static Action<object, BinaryWriter> GetClassSerializer(Type t)
        {
            // TODO: I want to use fields. 
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            // For classes we only serialize fields that can be read and written.
            var fields = t.GetFields(flags).ToList();
            var fieldSerializers = fields.Select(f => GetSerializer(f.FieldType)).ToList();

            return (o, bw) =>
            {
                if (o == null)
                {
                    bw.Write(0);
                }
                else
                {
                    bw.Write(1);

                    for (var i=0; i < fieldSerializers.Count; ++i)
                    {
                        var ps = fieldSerializers[i];
                        var p = fields[i];

                        // Call the property serializer
                        ps(p.GetValue(o), bw);
                    }
                }
            };
        }

        public static Dictionary<Type, Action<object, BinaryWriter>> SerializerTable = new Dictionary<Type, Action<object, BinaryWriter>>();

        public static Action<object, BinaryWriter> GetSerializer(this Type t)
        {
            return SerializerTable.GetOrCompute(t, ComputeSerializer);
        }

        public static void FastWriteBytes(BinaryWriter bw, Array xs)
        {
            using (var pin = xs.Pin())
                Win32FileIO.Write((bw.BaseStream as FileStream).SafeFileHandle, pin.Bytes);
        }

        public static Action<object, BinaryWriter> ComputeSerializer(this Type t)
        {
            switch (t.Name)
            {
                case "Byte": return (o, bw) => bw.Write((byte)o);
                case "Char": return (o, bw) => bw.Write((char)o);
                case "Int16": return (o, bw) => bw.Write((short)o);
                case "Int32": return (o, bw) => bw.Write((int)o);
                case "Int64": return (o, bw) => bw.Write((long)o);
                case "UInt16": return (o, bw) => bw.Write((ushort)o);
                case "UInt32": return (o, bw) => bw.Write((uint)o);
                case "Uint64": return (o, bw) => bw.Write((ulong)o);
                case "Single": return (o, bw) => bw.Write((float)o);
                case "Double": return (o, bw) => bw.Write((double)o);
                case "Boolean": return (o, bw) => bw.Write((bool)o);
                case "String": return (o, bw) => bw.Write(o as string ?? "");
            }

            if (t.IsPrimitive)
                throw new Exception($"Unandled primitive: {t}");

            if (t.IsEnum)
                return (o, bw) => bw.Write((int)o);

            if (t == typeof(DateTime))
                return (o, bw) => bw.Write(o.ToString());

            if (t.IsArray && t.GetElementType().ContainsNoReferences() && t.IsBlittable())
            {
                return (o, bw) =>
                {
                    var xs = o as Array;
                    if (xs == null)
                    {
                        bw.Write(0);
                    }
                    else
                    {
                        bw.Write(xs.Length);
                        bw.WriteBytes(xs);
                        //FastWriteBytes(bw, xs);
                    }
                };
            }

            if (t.ImplementsIList())
            {
                var genArg = t.GetGenericArguments()[0];
                var elementSerializer = GetSerializer(genArg);

                return (o, bw) =>
                {
                    var xs = (IList)o;
                    if (xs == null)
                    {
                        bw.Write(0);
                    }
                    else
                    {
                        bw.Write(xs.Count);
                        for (var i = 0; i < xs.Count; ++i)
                            elementSerializer(xs[i], bw);
                    }
                };
            }

            if (t.IsClass && t.Namespace.StartsWith("System", StringComparison.Ordinal))
                throw new Exception($"Unhandled System class type: {t}");

            if (t.IsClass | t.IsValueType)
            {
                return GetClassSerializer(t);
            }

            throw new Exception($"Serializer does not handle type {t}");
        }       
    }
}

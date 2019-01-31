/*
    G3D Data Format Library
    Copyright 2018, Ara 3D, Inc.
    Usage licensed under terms of MIT License
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Ara3D.G3D
{
    // NOTE: this has to be kept in synch with g3d.h 

    // The different types of data types that can be used as elements. 
    public enum DataType
    {
        dt_uint8,
        dt_uint16,
        dt_uint32,
        dt_uint64,
        dt_uint128,
        dt_int8,
        dt_int16,
        dt_int32,
        dt_int64,
        dt_int128,
        dt_float16, 
        dt_float32,
        dt_float64,
        dt_float128,
        dt_invalid,
    };

    // What element each attribute is associated with 
    public enum Association
    {
        assoc_vertex,
        assoc_face,
        assoc_corner,
        assoc_edge,
        assoc_object,
        assoc_none,
        assoc_invalid,
    };

    // The type of the attribute
    public enum AttributeType
    {
        attr_unknown,
        attr_user,
        attr_vertex,
        attr_index,
        attr_faceindex,
        attr_facesize,
        attr_normal,
        attr_binormal,
        attr_tangent,
        attr_materialid,
        attr_polygroup,
        attr_uv,
        attr_color,
        attr_smoothing,
        attr_crease,
        attr_hole,
        attr_visibility,
        attr_selection,
        attr_pervertex,
        attr_mapchannel_data,
        attr_mapchannel_index,
        attr_custom,
        attr_invalid,
    };

    // Contains all the information necessary to parse an attribute data channel and associate it with geometry 
    [StructLayout(LayoutKind.Sequential, Pack = 0, Size = 32)]
    public struct AttributeDescriptor
    {
        public int _association;            // Indicates the part of the geometry that this attribute is assoicated with 
        public int _attribute_type;         // n integer values 
        public int _attribute_type_index;   // each attribute type should have it's own index ( you can have uv0, uv1, etc. )
        public int _data_arity;             // how many values associated with each element (e.g. UVs might be 2, geometry might be 3, quaternions 4, matrices 9 or 16)
        public int _data_type;              // the type of individual values (e.g. int32, float64)
        public int _pad0, _pad1, _pad2;     // ignored, used to bring the alignment up to a power of two.
    }

    public interface IAttribute
    {
        byte[] Data { get; }
        AttributeDescriptor Descriptor { get; }
    }

    public class AttributeData<T> : IAttribute where T : struct 
    {
        public byte[] Data { get; }
        public AttributeDescriptor Descriptor { get; }
    
        public AttributeData(T[] data, AttributeDescriptor descriptor)
        {
            var span = data.AsSpan();
            var bytesSpan = MemoryMarshal.Cast<T, byte>(span);
            Data = new byte[bytesSpan.Length];
            bytesSpan.CopyTo(Data);
            Descriptor = descriptor;
        }
    }

    public static class AttributeBuilder
    {
        public static AttributeData<T> Attribute<T>(T[] data, AttributeDescriptor desc) where T: struct
        {
            return new AttributeData<T>(data, desc);
        }

        public static IAttribute Vertices(Vector3[] data)
        {
            return Attribute(data, Association.assoc_vertex, AttributeType.attr_vertex);
        }

        public static IAttribute Indices(int[] data)
        {
            return Attribute(data, Association.assoc_corner, AttributeType.attr_index);
        }

        public static IAttribute UVs(Vector2[] data, int index = 0)
        {
            return Attribute(data, Association.assoc_corner, AttributeType.attr_index);
        }

        public static IAttribute UVWs(Vector3[] data, int index = 0)
        {
            return Attribute(data, Association.assoc_vertex, AttributeType.attr_index);
        }

        public static IAttribute VertexNormals(Vector3[] data, int index = 0)
        {
            return Attribute(data, Association.assoc_vertex, AttributeType.attr_index);
        }

        public static AttributeData<T> Attribute<T>(T[] data, Association assoc, AttributeType at, int index = 0) where T : struct
        {
            return new AttributeData<T>(data, Descriptor<T>(assoc, at, index));
        }

        public static AttributeDescriptor Descriptor<T>(Association assoc, AttributeType at, int index = 0) where T : struct
        {
            if (typeof(T) == typeof(float))
                return Descriptor(assoc, at, index, DataType.dt_float32, 1);
            if (typeof(T) == typeof(double))
                return Descriptor(assoc, at, index, DataType.dt_float64, 1);
            if (typeof(T) == typeof(int))
                return Descriptor(assoc, at, index, DataType.dt_int32, 1);
            if (typeof(T) == typeof(long))
                return Descriptor(assoc, at, index, DataType.dt_int64, 1);
            if (typeof(T) == typeof(Vector2))
                return Descriptor(assoc, at, index, DataType.dt_float32, 2);
            if (typeof(T) == typeof(Vector3))
                return Descriptor(assoc, at, index, DataType.dt_float32, 3);
            if (typeof(T) == typeof(Vector4))
                return Descriptor(assoc, at, index, DataType.dt_float32, 4);
            if (typeof(T) == typeof(Quaternion))
                return Descriptor(assoc, at, index, DataType.dt_float32, 4);
            if (typeof(T) == typeof(Matrix4x4))
                return Descriptor(assoc, at, index, DataType.dt_float32, 16);
            throw new Exception($"Unhandled type {typeof(T)}");
        }

        public static AttributeDescriptor Descriptor(Association assoc, AttributeType at, int index, DataType dt, int arity)
        {
            return new AttributeDescriptor
            {
                _association = (int) assoc,
                _attribute_type = (int) at,
                _data_arity = (int) arity,
                _data_type = (int) dt,
            };
        }
    }

    /*
    public class G3DBuilder
    {
        public static BFast ToBFast(IEnumerable<IAttribute> attributes)
        {
            var descriptors = attributes.Select(a => a.Descriptor).ToArray();
            var descBytes = descriptors.ToBytes();
            var header = "{ type: \"g3d\" }";
            var buffers = descBytes.ToByteRange();
            BFastBuilder.ToBFastBytes(
        }

        // https://stackoverflow.com/questions/7956167/how-can-i-quickly-read-bytes-from-a-memory-mapped-file-in-net
        public static G3D Read(string filePath)
        {
            using (var file = new BFastMemoryMappedFile(filePath))
            {
                // TODO: map to a bfast struct
                // TODO: get the attributes list 
                // TODO: get all the arrays 
                // TODO: load each array 
            }
        }
    }*/
}

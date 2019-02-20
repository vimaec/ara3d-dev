using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Ara3D
{
    public static class G3DExtensions
    {          
        public static IAttribute ToAttribute<T>(this IArray<T> xs, AttributeDescriptor desc) where T: struct 
            => new AttributeArray<T>(xs, desc);

        public static IAttribute ToAttribute(this IBytes bytes, AttributeDescriptor desc) 
            => new AttributeBytes(bytes, desc);

        public static IAttribute ToAttribute<T>(this T[] data, AttributeDescriptor desc) where T : struct
            => data.Pin().ToAttribute(desc);
               
        public static IAttribute ToAttribute<T>(this T[] data, Association assoc, AttributeType at, int index = 0) where T : struct
            => data.ToAttribute(Descriptor<T>(assoc, at, index));

        public static IAttribute ToAttribute<T>(this IArray<T> data, Association assoc, AttributeType at, int index = 0) where T : struct
            => data.ToAttribute(Descriptor<T>(assoc, at, index));

        public static IAttribute ToFaceSizeAttribute(this int data)
            => new[] {data}.ToAttribute(Association.assoc_object, AttributeType.attr_facesize);

        public static IAttribute ToFaceSizeAttribute(this int[] data)
            => data.ToAttribute(Association.assoc_object, AttributeType.attr_facesize);

        public static IAttribute ToFaceSizeAttribute(this IArray<int> data)
            => data.ToAttribute(Association.assoc_object, AttributeType.attr_facesize);

        public static IAttribute ToVertexAttribute(this Vector3[] data)
            => data.ToAttribute(Association.assoc_vertex, AttributeType.attr_vertex);

        public static IAttribute ToVertexAttribute(this IArray<Vector3> data)
            => data.ToAttribute(Association.assoc_vertex, AttributeType.attr_vertex);

        public static IAttribute ToVertexAttribute(this DVector3[] data)
            => data.ToAttribute(Association.assoc_vertex, AttributeType.attr_vertex);

        public static IAttribute ToVertexAttribute(this IArray<DVector3> data)
            => data.ToAttribute(Association.assoc_vertex, AttributeType.attr_vertex);

        public static IAttribute ToIndexAttribute(this int[] data)
            => data.ToAttribute(Association.assoc_corner, AttributeType.attr_index);

        public static IAttribute ToIndexAttribute(this IArray<int> data)
            => data.ToAttribute(Association.assoc_corner, AttributeType.attr_index);

        public static IAttribute ToIndexAttribute(this short[] data)
            => data.ToAttribute(Association.assoc_corner, AttributeType.attr_index);

        public static IAttribute ToIndexAttribute(this IArray<short> data)
            => data.ToAttribute(Association.assoc_corner, AttributeType.attr_index);

        public static IAttribute ToIndexAttribute(this byte[] data)
            => data.ToAttribute(Association.assoc_corner, AttributeType.attr_index);

        public static IAttribute ToIndexAttribute(this IArray<byte> data)
            => data.ToAttribute(Association.assoc_corner, AttributeType.attr_index);

        public static IAttribute ToUvAttribute(this Vector2[] data, int index = 0)
            => data.ToAttribute(Association.assoc_corner, AttributeType.attr_uv, index);

        public static IAttribute ToUvAttribute(this IArray<Vector2> data, int index = 0)
            => data.ToAttribute(Association.assoc_corner, AttributeType.attr_uv, index);

        public static IAttribute ToUvwAttribute(this Vector3[] data, int index = 0)
            => data.ToAttribute(Association.assoc_vertex, AttributeType.attr_uv, index);

        public static IAttribute ToUvwAttribute(this IArray<Vector3> data, int index = 0)
            => data.ToAttribute(Association.assoc_vertex, AttributeType.attr_uv, index);

        public static IAttribute ToVertexNormalAttribute(this Vector3[] data, int index = 0)
            => data.ToAttribute(Association.assoc_vertex, AttributeType.attr_normal, index);

        public static IAttribute ToVertexNormalAttribute(this IArray<Vector3> data, int index = 0)
            => data.ToAttribute(Association.assoc_vertex, AttributeType.attr_normal, index);

        public static IEnumerable<AttributeDescriptor> Descriptors(this IG3D g3D)
            => g3D.Attributes.Select(attr => attr.Descriptor);

        public static IEnumerable<IAttribute> FindAttributes(this IG3D g3D, Func<AttributeDescriptor, bool> predicate)
            => g3D.Attributes.Where(a => predicate(a.Descriptor));

        public static IEnumerable<IAttribute> FindAttributes(this IG3D g3D, AttributeType attributeType)
            => g3D.FindAttributes(desc => desc.AttributeType == attributeType);

        public static string AttributeTypeString(this AttributeType at)
            => AttributeDescriptor.AttributeTypeToString((int)at);        

        public static IAttribute FindAttribute(this IG3D g3D, AttributeType attributeType, bool throwIfMissing = true)
        {
            var candidates = g3D.FindAttributes(attributeType).ToList();
            if (candidates.Count > 1)
                throw new Exception($"Multiple matching attributes of type {attributeType.AttributeTypeString()}");
            if (candidates.Count == 0)
            {
                if (throwIfMissing)
                    throw new Exception($"No matching attributes of type {attributeType.AttributeTypeString()}");
                return null;
            }
            return candidates[0];
        }

        public static Type ToType(this DataType dt)
        {
            switch (dt)
            {
                case DataType.dt_int8:
                    return typeof(byte);
                case DataType.dt_int16:
                    return typeof(short);
                case DataType.dt_int32:
                    return typeof(int);
                case DataType.dt_int64:
                    return typeof(long);
                case DataType.dt_float32:
                    return typeof(float);
                case DataType.dt_float64:
                    return typeof(double);
                case DataType.dt_invalid:
                    throw new Exception("Not a valid data type");
                default:
                    throw new ArgumentOutOfRangeException(nameof(dt), dt, null);
            }
        }

        public static AttributeDescriptor Descriptor<T>(Association assoc, AttributeType at, int index = 0) where T : struct
        {
            if (typeof(T) == typeof(float))
                return Descriptor(assoc, at, index, DataType.dt_float32, 1);
            if (typeof(T) == typeof(double))
                return Descriptor(assoc, at, index, DataType.dt_float64, 1);
            if (typeof(T) == typeof(short))
                return Descriptor(assoc, at, index, DataType.dt_int16, 1);
            if (typeof(T) == typeof(byte))
                return Descriptor(assoc, at, index, DataType.dt_int8, 1);
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
            if (typeof(T) == typeof(DVector2))
                return Descriptor(assoc, at, index, DataType.dt_float64, 2);
            if (typeof(T) == typeof(DVector3))
                return Descriptor(assoc, at, index, DataType.dt_float64, 3);
            if (typeof(T) == typeof(DVector4))
                return Descriptor(assoc, at, index, DataType.dt_float64, 4);
            if (typeof(T) == typeof(Matrix4x4))
                return Descriptor(assoc, at, index, DataType.dt_float32, 16);
            throw new Exception($"Unhandled type {typeof(T)}");
        }

        public static AttributeDescriptor Descriptor(Association assoc, AttributeType at, int index, DataType dt, int arity)
            => new AttributeDescriptor {
                _association = (int) assoc,
                _attribute_type = (int) at,
                _data_arity = arity,
                _data_type = (int) dt,
            };        

        public static G3D ReadFromFile(string filePath) 
            => G3D.Create(File.ReadAllBytes(filePath));

        public static G3D ToG3D(this BFast bfast)
            => G3D.Create(bfast);
        
        public static BFast ToBFast(this IG3D g3D)
        {
            if (g3D is G3D g)
                return g.Buffer;
            var buffers = new List<IBytes>();
            buffers.Add(G3D.DefaultHeader.ToBytesAscii().Pin());
            var descriptors = g3D.Descriptors().ToArray().Pin();
            buffers.Add(descriptors);
            foreach (var attr in g3D.Attributes)
                buffers.Add(attr.Bytes);
            return new BFast(buffers);
        }
             
        public static void WriteToFile(this IG3D g3D, string filePath) 
            => g3D.ToBFast().WriteToFile(filePath);

        public static IG3D ToG3D(this IEnumerable<IAttribute> attributes)
            => new G3D(attributes.WhereNotNull());

        public static IG3D ToG3D(params IAttribute[] attributes)
            => attributes.ToG3D();

        public static IG3D ToG3D(int sidesPerFaces, IArray<Vector3> vertices, IArray<int> indices = null)
            => ToG3D(sidesPerFaces.ToFaceSizeAttribute(), vertices.ToVertexAttribute(), indices?.ToIndexAttribute());

        public static IG3D ToG3D(int sidesPerFaces, Vector3[] vertices, int[] indices = null)
            => ToG3D(sidesPerFaces.ToFaceSizeAttribute(), vertices.ToVertexAttribute(), indices?.ToIndexAttribute());

        public static BFast ToBFast(this IEnumerable<IAttribute> attributes)
            => attributes.ToG3D().ToBFast();

        public static IEnumerable<IAttribute> AttributesExcept(this IG3D g3d, AttributeDescriptor desc)
            => g3d.Attributes.Where(attr => !attr.Descriptor.Equals(desc));

        public static IG3D RemoveAttribute(this IG3D g3d, AttributeDescriptor desc)
            => g3d.AttributesExcept(desc).ToG3D();

        public static IG3D ReplaceAttribute(this IG3D g3d, IAttribute attribute)
            => g3d.RemoveAttribute(attribute.Descriptor).AddAttributes(attribute);

        public static IG3D AddAttributes(this IG3D g3d, params IAttribute[] attributes)
            => g3d.Attributes.Concat(attributes).ToG3D();

        public static IEnumerable<IAttribute> Attributes(this IG3D g3d, Association assoc)
            => g3d.Attributes.Where(attr => attr.Descriptor.Association == assoc);

        public static IEnumerable<IAttribute> VertexAttributes(this IG3D g3d)
            => g3d.Attributes(Association.assoc_vertex);

        public static IEnumerable<IAttribute> CornerAttributes(this IG3D g3d)
            => g3d.Attributes(Association.assoc_corner);

        public static IEnumerable<IAttribute> FaceAttributes(this IG3D g3d)
            => g3d.Attributes(Association.assoc_face);

        public static IEnumerable<IAttribute> EdgeAttributes(this IG3D g3d)
            => g3d.Attributes(Association.assoc_edge);

        public static IEnumerable<IAttribute> ObjectAttributes(this IG3D g3d)
            => g3d.Attributes(Association.assoc_object);

        public static IEnumerable<IAttribute> NoneAttributes(this IG3D g3d)
            => g3d.Attributes(Association.assoc_none);

        public static IAttribute Remap(this IAttribute attr, IArray<int> indices)
        {
            var n = attr.Descriptor.DataArity;
            switch (attr.Descriptor.DataType)
            {
                case DataType.dt_int8:
                    return attr.ToBytes().SelectGroupsByIndex(n, indices).ToAttribute(attr.Descriptor);
                case DataType.dt_int16:
                    return attr.ToShorts().SelectGroupsByIndex(n, indices).ToAttribute(attr.Descriptor);                    
                case DataType.dt_int32:
                    return attr.ToInts().SelectGroupsByIndex(n, indices).ToAttribute(attr.Descriptor);
                case DataType.dt_int64:
                    return attr.ToLongs().SelectGroupsByIndex(n, indices).ToAttribute(attr.Descriptor);
                case DataType.dt_float32:
                    return attr.ToFloats().SelectGroupsByIndex(n, indices).ToAttribute(attr.Descriptor);
                case DataType.dt_float64:
                    return attr.ToDoubles().SelectGroupsByIndex(n, indices).ToAttribute(attr.Descriptor);
            }

            throw new Exception("Not a recognized data type");
        }
    }
}
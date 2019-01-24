using System;

namespace G3D
{
    public struct Header
    {
        public string FileTypeVersion { get; set; }
        public string Units { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Copyright { get; set; }
    }

    public enum ReferenceMode
    {
        Direct,
        IndexBuffer,
        DataBuffer,
    }

    public enum AttributeAssociation
    {
        Unknown = 0,
        Point = 1,
        Polygon = 2,
        PolygonVertex = 3,
        Edge = 4,
        Object = 5,
    }

    public enum DataTypeEnum
    {
        Float,
        Int,
        UnsignedInt, // unused
    }

    public struct BufferDescriptor
    {
        public string Name { get; }
        public DataTypeEnum DataType { get; }
        public AttributeAssociation Element { get; }
        public ReferenceMode Reference { get; }
        public int DataTypeByteSize { get; } // always "4" for
        public int Arity { get; }
    }

    public unsafe class Buffer
    {
        public BufferDescriptor Descriptor { get; }
        public int Count { get; }
        public byte* Data { get; }
    }   

    public class FloatBuffer {  }
    public class IntBuffer { } 


    /// <summary>
    /// A G3D file is a simple binary mesh file format that is both simple and efficient to read and write. 
    /// It supports lossless representation of all geometry stored in meshes in 3ds Max, which is generally 
    /// sufficient for transferring geometry data between many 3D tools and rendering engines.  
    /// </summary>
    public static class G3D
    {
        public const string VertexDataChannelName = "vertex_data";
        public const string MapChannelDataName = "map_channel_data_buffer";
        public const string MapChannelIndexName = "map_channel_index_buffer";

        public static BufferDescriptor GetDescriptor(string name)
        {
            switch (name)
            {
                case "material_id":
                case "vertex_buffer":
                case "index_buffer":
                case "smoothing_group":
                case "face_normal":
                case "vertex_normal":
                case "vertex_tangent":
                case "vertex_binormal":
                case "vertex_uv":
                case "vertex_uv2":
                case "edge_visibility":
                case "face_id":
                case "poly_size":
                    break;
            }

            if (name.StartsWith(VertexDataChannelName, StringComparison.Ordinal))
            {

            }
            else if (name.StartsWith(MapChannelDataName, StringComparison.Ordinal))
            {

            }
            else if (name.StartsWith(MapChannelIndexName, StringComparison.Ordinal))
            {

            }

            // TODO: handle map_channel_data_buffer, map_channel_index_buffer, 
            throw new NotImplementedException();
        }
    }
}

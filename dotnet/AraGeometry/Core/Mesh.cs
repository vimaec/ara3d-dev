
/*
namespace Ara3D
{
    public struct IntBuffer
    {
        public IArray<int> Values { get; }
    }

    public struct FloatBuffer
    {
        public IArray<float> Values { get; }
    }

    /// <summary>
    /// A map channel is a way of describing data 
    /// </summary>
    public struct MapChannel
    {
        public IntBuffer Indices;
        public FloatBuffer Data;
    }

    /// <summary>
    /// Enumeration of the standard map channels. 
    /// </summary>
    public enum StandardMapChannels
    {
        Geometry = 0,
        Selection = 1,
        Alpha = 2,          // 3ds Max Map Channel -2 
        Illumination = 3,   // 3ds Max Map Channel -1
        Color = 4,          // 3ds Max Map Channel 0,
        UVW = 5,            // 3ds Max Map Channel 1 a.k.a Default
    }

    /// <summary>
    /// This is a mesh representation that is adequate to store most data from 3ds Max, FBX, AssImp, 
    /// and many other 3D tools. All buffers are optional except for Vertices.
    /// Also related see: 
    /// http://assimp.sourceforge.net/lib_html/structai_mesh.html
    /// https://docs.unity3d.com/ScriptReference/Mesh.html
    /// https://help.autodesk.com/view/3DSMAX/2017/ENU/?guid=__cpp_ref_class_mesh_html
    /// https://help.autodesk.com/view/FBX/2017/ENU/?guid=__files_GUID_5EDC0280_E000_4B0B_88DF_5D215A589D5E_htm
    /// </summary>
    public class Mesh
    {
        /// <summary>
        /// Describes the type of mesh:
        /// * 0 - polygon mesh
        /// * 1 - point cloud
        /// * 2 - line segments
        /// * 3 - triangle mesh
        /// * 4 - quad mesh 
        /// </summary>
        public int FaceSize;

        /// <summary>
        /// Standard geometry data
        /// </summary>
        public FloatBuffer Vertices { get; set; }
        public IntBuffer Indices { get; set; }

        // These channels are required in PolyMeshes (non-tri and non-quad)
        public IntBuffer FaceCounts { get; set; }
        public IntBuffer FaceIndices { get; set; }
        //public IntBuffer FaceIds { get; set; } 

        // 3ds Max specific data buffers 
        public IntBuffer EdgeVisibility { get; set; }
        public IntBuffer MaterialIds { get; set; }
        public IntBuffer SmoothingGroups { get; set; }

        // Computed channcels 
        public FloatBuffer VertexNormals { get; set; }
        public FloatBuffer VertexTangents { get; set; }
        public FloatBuffer VertexBitangents { get; set; }
        public FloatBuffer FaceNormals { get; set; }
        
        public IntBuffer VertexLookup { get; }

        public FloatBuffer[] VertexData { get; }
        
        public bool IsMapChannelPresent(int id)
        {
            throw new NotImplementedException();
        }

        public IArray<MapChannel> Channels { get; } 
        public IArray<FloatBuffer> VertexBuffer { get; }

        // TODO: compute a SHA1 hash
        // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hmacsha1?view=netframework-4.7.2
        // public HMACSHA1 Hash() { }
    }
}
*/
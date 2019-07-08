using UnityEngine;
using System.Linq;

namespace Ara3D.UnityBridge
{
    /// <summary>
    /// A copy of the Topology, UVs, Vertices, Colors, Normals, and Tangenets of a Unity mesh.
    /// </summary>
    public class MeshClone 
    {
        public UnityEngine.Vector2[] UnityUVs;
        public UnityEngine.Vector3[] UnityVertices;
        public UnityEngine.Vector3[] UnityNormals;
        public UnityEngine.Vector4[] UnityTangents;
        public int[] UnityIndices;
        public Color32[] UnityColors;

        public MeshClone(MeshClone other)
        {
            CopyFrom(other);
        }

        public void CopyFrom(MeshClone other)
        {
            UnityIndices = other.UnityIndices;
            UnityUVs = other.UnityUVs?.ToArray();
            UnityVertices = other.UnityVertices?.ToArray();
            UnityColors = other.UnityColors?.ToArray();
            UnityNormals = other.UnityNormals?.ToArray();
            UnityTangents = other.UnityTangents?.ToArray();
        }

        public void CopyFrom(Mesh mesh)
        {
            UnityIndices = mesh.triangles;
            UnityUVs = mesh.uv?.ToArray();
            UnityVertices = mesh.vertices?.ToArray();
            UnityColors = mesh.colors32?.ToArray();
            UnityNormals = mesh.normals?.ToArray();
            UnityTangents = mesh.tangents?.ToArray();
        }

        public MeshClone(Mesh mesh)
        {
            CopyFrom(mesh);
        }

        public void AssignToMesh(Mesh mesh)
        {
            // NOTE: maybe this could be optimized, so that only changed data is copied over 
            mesh.Clear();
            if (UnityVertices != null) mesh.vertices = UnityVertices;
            if (UnityIndices != null) mesh.triangles = UnityIndices;
            if (UnityUVs != null) mesh.uv = UnityUVs;
            if (UnityColors != null) mesh.colors32 = UnityColors;
            if (UnityNormals != null) mesh.normals = UnityNormals;
            if (UnityTangents != null) mesh.tangents = UnityTangents;
        }

        public MeshClone Clone()
        {
            return new MeshClone(this);
        }
    }
}

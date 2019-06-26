using UnityEngine;
using System.Linq;

namespace Ara3D.UnityBridge
{
    /// <summary>
    /// A copy of the UVs, Vertices, Colors, Normals, and Tangenets of a Unity mesh.
    /// Topology is assumed to be constant. 
    /// </summary>
    public class MeshClone
    {
        public UnityEngine.Vector2[] UVs;
        public UnityEngine.Vector3[] Vertices;
        public UnityEngine.Vector3[] Normals;
        public UnityEngine.Vector4[] Tangents;
        public Color32[] Colors;

        public MeshClone(MeshClone other)
        {
            CopyFrom(other);
        }

        public void CopyFrom(MeshClone other)
        {
            UVs = other.UVs?.ToArray();
            Vertices = other.Vertices?.ToArray();
            Colors = other.Colors?.ToArray();
            Normals = other.Normals?.ToArray();
            Tangents = other.Tangents?.ToArray();
        }

        public void CopyFrom(Mesh mesh)
        {
            UVs = mesh.uv?.ToArray();
            Vertices = mesh.vertices?.ToArray();
            Colors = mesh.colors32?.ToArray();
            Normals = mesh.normals?.ToArray();
            Tangents = mesh.tangents?.ToArray();
        }

        public MeshClone(Mesh mesh)
        {
            CopyFrom(mesh);
        }

        public void AssignToMesh(Mesh mesh)
        {
            // NOTE: maybe this could be optimized, so that only changed data is copied over 
            if (Vertices != null) mesh.vertices = Vertices;
            if (UVs != null) mesh.uv = UVs;
            if (Colors != null) mesh.colors32 = Colors;
            if (Normals != null) mesh.normals = Normals;
            if (Tangents != null) mesh.tangents = Tangents;
        }

        public MeshClone Clone()
        {
            return new MeshClone(this);
        }
    }

    /// <summary>
    /// Manipulate the Buffer mesh to your heart's content, and call "UpdateTarget" whenever you want. 
    /// </summary>
    public class ProceduralMesh
    {
        public MeshClone Original { get; private set; }
        public MeshClone Buffer { get; private set; }
        public Mesh Target { get; private set; }

        public ProceduralMesh(Mesh mesh)
        {
            Target = mesh;
            Original = new MeshClone(Target);
            Buffer = Original.Clone();  
        }

        public void ResetTarget()
        {
            Buffer.CopyFrom(Original);
            UpdateTarget();
        }

        public void UpdateTarget()
        {
            Buffer.AssignToMesh(Target);
        }
    }
}

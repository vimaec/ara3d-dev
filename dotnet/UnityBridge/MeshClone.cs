using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Geometry3D;
using System;

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

        // TODO: this needs to be improved (assumes TriMesh, etc.)
        public void FromGeometry(IGeometry g)
        {
            if (g.PointsPerFace != 3) throw new Exception("Only triangle meshes are handled");
            UnityIndices = g.Indices.ToArray();
            UnityVertices = g.Vertices.Select(UnityConverters.ToUnity).ToArray();
        }

        // TODO: this needs to be improved: doesn't handle colors, normals, tangents, etc. 
        public IGeometry ToGeometry()
            => UnityVertices.ToAra3D().TriMesh(UnityIndices.ToIArray(), UnityUVs.ToAra3D());

        /*
        public IArray<int> Indices => UnityIndices.ToIArray();
        public IArray<int> FaceSizes => 3.Repeat(NumFaces);
        public IArray<Vector2> UVs => UnityUVs.ToAra3D();
        private Topology _topo;
        public Topology Topology => _topo ?? (_topo = new Topology(this));
        public IEnumerable<IAttribute> Attributes => throw new System.NotImplementedException();
        public IAttribute VertexAttribute => throw new System.NotImplementedException();
        public IAttribute IndexAttribute => throw new System.NotImplementedException();
        public IAttribute FaceSizeAttribute => throw new System.NotImplementedException();
        public IAttribute FaceIndexAttribute => throw new System.NotImplementedException();
        public IAttribute MaterialIdAttribute => throw new System.NotImplementedException();
        */

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

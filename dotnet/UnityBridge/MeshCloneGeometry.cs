using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ara3D.UnityBridge
{
    /// <summary>
    /// Adapts a mesh clone (which is a copy of a Unity mesh) so that it implements IGeometry    
    /// </summary>
    public class MeshCloneGeometry : IGeometry
    {
        public MeshClone Mesh;

        public MeshCloneGeometry(MeshClone mc)
        {
            NumFaces = mc.UnityIndices.Length / 3;
            Vertices = mc.UnityVertices.ToIArray().Select(v => v.ToAra3D());
            Indices = mc.UnityIndices.ToIArray();
        }

        public int PointsPerFace { get; } = 3;
        public int NumFaces { get; }
        public IArray<Vector3> Vertices { get; }
        public IArray<int> Indices { get; }
        public IArray<int> FaceSizes { get; }
        public IArray<Vector2> UVs { get; }
        public Topology Topology { get; }
        public IEnumerable<IAttribute> Attributes { get; }
        public IAttribute VertexAttribute { get; }
        public IAttribute IndexAttribute { get; }
        public IAttribute FaceSizeAttribute { get; }
        public IAttribute FaceIndexAttribute { get; }
        public IAttribute MaterialIdAttribute { get; }
    }
}

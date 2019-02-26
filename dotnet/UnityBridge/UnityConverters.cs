using Ara3D;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace UnityBridge
{
    // TODO: add an IGeometry wrapper around "Unity"
    public static class UnitYConverters
    {
        //public static UnityEngine.Vector2 ToUnity(this Vector2 v)
        //    => new UnityEngine.Vector2(v.X, v.Y);

        //public static UnityEngine.Vector3 ToUnity(this Vector3 v)
        //    => new UnityEngine.Vector3(v.X, v.Y, v.Z);

        //public static UnityEngine.Vector4 ToUnity(this Vector4 v)
        //    => new UnityEngine.Vector4(v.X, v.Y, v.Z, v.W);

        public static UnityEngine.Vector3 ToUnity(Vector3 v)
            => new UnityEngine.Vector3(v.X, v.Y, v.Z);


        public static UnityEngine.Mesh ToUnity(this IGeometry g)
        {
            var r = new UnityEngine.Mesh();
            
            // TODO: I think we should be able to cast the arrays from one type to another
            r.vertices = g.Vertices.Select(ToUnity).ToArray();

            if (g.PointsPerFace == 3)
                r.SetIndices(g.Indices.ToArray(), MeshTopology.Triangles, 0);
            else if (g.PointsPerFace == 4)
                r.SetIndices(g.Indices.ToArray(), MeshTopology.Quads, 0);
            else if (g.PointsPerFace == 2)
                // TODO: what exactly is a MeshTopology.Lines or MeshTopology.Points for Unity
                r.SetIndices(g.Indices.ToArray(), MeshTopology.Lines, 0);
            else if (g.PointsPerFace == 1)
                r.SetIndices(g.Indices.ToArray(), MeshTopology.Points, 0);
            else
            {
                var triMesh = g.ToTriMesh();
                r.SetIndices(triMesh.Indices.ToArray(), MeshTopology.Triangles, 0);
            }

            // TODO: copy colors, normals, uvs1 through 8, tangents, and boneWeights
            //r.colors = g.VertexColors.Select();
            //r.normals = g.VertexNormals.Select();
            // r.uv(8) = g.UV.Select();
            //r.tangents;
            //https://docs.unity3d.com/ScriptReference/BoneWeight.html
            // r.boneWeights
            return r;
        }
    }
}

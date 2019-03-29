using System;
using Ara3D;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityBridge
{
    // TODO: add an IGeometry wrapper around "Unity"
    public static class UnityConverters
    {
        // When translating G3D faces to unity we need
        // to reverse the triangle winding.
        public static int PolyFaceToUnity(int index, int faceSize)
        {
            var faceIdx = index / faceSize;
            var vertIdx = index % faceSize;
            return (vertIdx == 0) ? index : (faceIdx * faceSize) + (faceSize - vertIdx);
        }

        // Remaps 1, 2, 3 to 1, 3, 2
        public static int TriFaceToUnity(int index)
            => PolyFaceToUnity(index, 3);

        public static int QuadFaceToUnity(int index)
            => PolyFaceToUnity(index, 4);

        public static Mesh ToUnity(this IGeometry g)
        {
            var r = new Mesh();
            
            var indices = g.IndexAttribute.ToInts();
            var vertexFloats = g.VertexAttribute.ToFloats();
            var unityVertices = vertexFloats.SelectTriplets((x, y, z) => new UnityEngine.Vector3(-x, z, -y));
            r.vertices = unityVertices.ToArray();
            r.indexFormat = unityVertices.Count > ushort.MaxValue
                ? IndexFormat.UInt32
                : IndexFormat.UInt16;

            switch (g.PointsPerFace)
            {
                case 3:
                    {
                        var unityIndices = indices.MapIndices(TriFaceToUnity);
                        r.SetIndices(unityIndices.ToArray(), MeshTopology.Triangles, 0);
                        break;
                    }
                case 4:
                    {
                        var unityIndices = indices.MapIndices(QuadFaceToUnity);
                        r.SetIndices(unityIndices.ToArray(), MeshTopology.Quads, 0);
                        break;
                    }
                case 2:
                    r.SetIndices(g.Indices.ToArray(), MeshTopology.Lines, 0);
                    break;
                case 1:
                    r.SetIndices(g.Indices.ToArray(), MeshTopology.Points, 0);
                    break;
                default:
                    {
                        var triMesh = g.ToTriMesh();
                        var unityIndices = triMesh.Indices.MapIndices(TriFaceToUnity);
                        r.SetIndices(unityIndices.ToArray(), MeshTopology.Triangles, 0);
                        break;
                    }
            }


            r.RecalculateNormals();
            return r;

            // TODO: copy colors, normals, uvs1 through 8, tangents, and boneWeights
            //r.colors = g.VertexColors.Select();
            //r.normals = g.VertexNormals.Select();
            // r.uv(8) = g.UV.Select();
            //r.tangents;
            //https://docs.unity3d.com/ScriptReference/BoneWeight.html
            // r.boneWeights
            //return r;
        }

        /*
        public static Mesh ToUnity(this IGeometry g)
        {
            var r = new Mesh();

            var indices = g.IndexAttribute.ToInts();
            var vertexFloats = g.VertexAttribute.ToFloats();
            var unityVertices = vertexFloats.SelectTriplets((x, y, z) => new UnityEngine.Vector3(-x, z, -y));
            r.vertices = unityVertices.ToArray();
            r.indexFormat = unityVertices.Count > ushort.MaxValue 
                ? IndexFormat.UInt32 
                : IndexFormat.UInt16;

            switch (g.PointsPerFace)
            {
                case 3:
                    {
                        var unityIndices = indices.MapIndices(TriFaceToUnity);
                        r.SetIndices(unityIndices.ToArray(), MeshTopology.Triangles, 0);
                        break;
                    }
                case 4:
                    {
                        var unityIndices = indices.MapIndices(QuadFaceToUnity);
                        r.SetIndices(unityIndices.ToArray(), MeshTopology.Quads, 0);
                        break;
                    }
                case 2:
                    r.SetIndices(g.Indices.ToArray(), MeshTopology.Lines, 0);
                    break;
                case 1:
                    r.SetIndices(g.Indices.ToArray(), MeshTopology.Points, 0);
                    break;
                default:
                    {
                        var triMesh = g.ToTriMesh();
                        var unityIndices = triMesh.Indices.MapIndices(TriFaceToUnity);
                        r.SetIndices(unityIndices.ToArray(), MeshTopology.Triangles, 0);
                        break;
                    }
            }


            r.RecalculateNormals();
            return r;

            // TODO: copy colors, normals, uvs1 through 8, tangents, and boneWeights
            //r.colors = g.VertexColors.Select();
            //r.normals = g.VertexNormals.Select();
            // r.uv(8) = g.UV.Select();
            //r.tangents;
            //https://docs.unity3d.com/ScriptReference/BoneWeight.html
            // r.boneWeights
            //return r;
        }
        */

        public static void SetFromNode(this Transform transform, ISceneNode node)
            => transform.SetFromMatrix(node.Transform);

        public static void SetFromMatrix(this Transform transform, Ara3D.Matrix4x4 matrix)
        {
            if (!Ara3D.Matrix4x4.Decompose(matrix, out var scl, out var rot, out var pos))
                throw new Exception("Can't decompose matrix");

            // Transform space is mirrored on X, and then rotated 90 degrees around X
            transform.position = new UnityEngine.Vector3(-pos.X, pos.Z, -pos.Y);
            // Quaternion is mirrored the same way, but then negated via W = -W because that's just easier to read
            transform.rotation = new UnityEngine.Quaternion(rot.X, -rot.Z, rot.Y, rot.W);
        }

    }
}

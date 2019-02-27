using System;
using System.Collections.Generic;
using Ara3D;
using UnityEngine;
using UnityEngine.Rendering;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace UnityBridge
{
    // TODO: add an IGeometry wrapper around "Unity"
    public static class UnitYConverters
    {
        // Remaps 1, 2, 3 to 1, 3, 2
        public static int TriFaceToUnity(int index)
        {
            var faceIdx = index / 3;
            var vertIdx = index % 3;
            return (vertIdx == 0) ? index : (faceIdx * 3) + (3 - vertIdx);
        }

        public static Mesh ToUnity(this IGeometry g)
        {
            var r = new Mesh();

            var indices = g.IndexAttribute.ToInts();
            var vertexFloats = g.VertexAttribute.ToFloats();
            // NOTE!  There is an issue with the VertexFloats array
            var unityVertices = vertexFloats.SelectTriplets((x, y, z) => new UnityEngine.Vector3(-x, y, z));
            r.vertices = unityVertices.ToArray();
            r.indexFormat = unityVertices.Count > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16;


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
                        //var unityIndices = indices.SelectQuartets((x, y, z, w) => (x, w, z, y)).ToInts();
                        //r.SetIndices(unityIndices.ToArray(), MeshTopology.Quads, 0);
                        break;
                    }
                // TODO: what exactly is a MeshTopology.Lines or MeshTopology.Points for Unity
                case 2:
                    r.SetIndices(g.Indices.ToArray(), MeshTopology.Lines, 0);
                    break;
                case 1:
                    r.SetIndices(g.Indices.ToArray(), MeshTopology.Points, 0);
                    break;
                default:
                    {
                        var triMesh = g.ToTriMesh();
                        var unityIndices = triMesh.Indices.SelectTriplets((x, y, z) => (x, z, y)).ToInts();
                        r.SetIndices(unityIndices.ToArray(), MeshTopology.Triangles, 0);
                        //r.SetIndices(triMesh.Indices.ToArray(), MeshTopology.Triangles, 0);
                        break;
                    }
            }

            
            //mesh.SetIndices(indices, MeshTopology.Triangles, 0, true);
            r.RecalculateNormals();
            return r;

            //g.VertexAttribute;

            //// TODO: I think we should be able to cast the arrays from one type to another
            //r.vertices = g.Vertices.Select(ToUnity).ToArray();

            //if (g.PointsPerFace == 3)
            //    r.SetIndices(g.Indices.ToArray(), MeshTopology.Triangles, 0);
            //else if (g.PointsPerFace == 4)
            //    r.SetIndices(g.Indices.ToArray(), MeshTopology.Quads, 0);
            //else if (g.PointsPerFace == 2)
            //    // TODO: what exactly is a MeshTopology.Lines or MeshTopology.Points for Unity
            //    r.SetIndices(g.Indices.ToArray(), MeshTopology.Lines, 0);
            //else if (g.PointsPerFace == 1)
            //    r.SetIndices(g.Indices.ToArray(), MeshTopology.Points, 0);
            //else
            //{
            //    var triMesh = g.ToTriMesh();
            //    r.SetIndices(triMesh.Indices.ToArray(), MeshTopology.Triangles, 0);
            //}

            // TODO: copy colors, normals, uvs1 through 8, tangents, and boneWeights
            //r.colors = g.VertexColors.Select();
            //r.normals = g.VertexNormals.Select();
            // r.uv(8) = g.UV.Select();
            //r.tangents;
            //https://docs.unity3d.com/ScriptReference/BoneWeight.html
            // r.boneWeights
            //return r;
        }
    }
}

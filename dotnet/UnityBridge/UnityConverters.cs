﻿using System;
using System.Collections.Generic;
using Ara3D;
using Ara3D.Revit.DataModel;
using UnityEngine;
using UnityEngine.Rendering;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

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

        // TODO: support matrices once we have our own math classes
        //public static void SetAra3DMatrix(this Transform transform, System.Numerics.Matrix4x4 mat)
        //    => transform.SetAra3DMatrix(mat.ToFloats());

        public static void SetAra3DMatrix(this Transform transform, float[] mtx)
        {
            transform.position = new UnityEngine.Vector3(-mtx[12], mtx[13], mtx[14]);

            // TODO: Mat to quat
        }

        public static void SetFromNode(this Transform transform, ISceneNode node)
        {
            // TODO: Strong assumption - The coordinate system of the ISceneNode's Transform matches that of Revit.
            transform.SetFromRevitMatrix(node.Transform);
        }

        public static void SetFromRevitSceneNode(this Transform transform, RevitSceneNode node)
        {
            transform.SetFromRevitMatrix(node.Transform);
        }

        public static void SetFromRevitMatrix(this Transform transform, System.Numerics.Matrix4x4 matrix)
        {
            if (!matrix.UnityPRS(out var pos, out var rot, out var scl))
                throw new Exception("Can't decompose matrix");

            RevitToUnityCoords(pos, rot, scl, out var outPos, out var outRot, out var outScl);

            transform.position = outPos;
            transform.rotation = outRot;
            transform.localScale = outScl;
        }

        /// <summary>
        /// Converts the given Revit-based coordinates into Unity coordinates.
        /// </summary>
        public static void RevitToUnityCoords(
            UnityEngine.Vector3 pos,
            UnityEngine.Quaternion rot,
            UnityEngine.Vector3 scale,
            out UnityEngine.Vector3 outPos,
            out UnityEngine.Quaternion outRot,
            out UnityEngine.Vector3 outScale)
        {
            // Transform space is mirrored on X, and then rotated 90 degrees around X
            outPos = new UnityEngine.Vector3(-pos.x, pos.z, -pos.y);

            // Quaternion is mirrored the same way, but then negated via W = -W because that's just easier to read
            outRot = new Quaternion(rot.x, -rot.z, rot.y, rot.w);

            // TODO: test this, current scale is completely untested
            outScale = new UnityEngine.Vector3(scale.x, scale.z, scale.y);
        }

        /// <summary>
        /// Extracts the Unity-compatible types for position, rotation, and scale from the given matrix.
        /// Returns false if the matrix cannot be decomposed.
        /// </summary>
        public static bool UnityPRS(
            this System.Numerics.Matrix4x4 matrix,
            out UnityEngine.Vector3 position,
            out UnityEngine.Quaternion rotation,
            out UnityEngine.Vector3 scale)
        {
            position = new UnityEngine.Vector3();
            rotation = new UnityEngine.Quaternion();
            scale = new UnityEngine.Vector3(1, 1, 1);

            if (matrix.IsIdentity)
                return true;

            var decomposed = System.Numerics.Matrix4x4.Decompose(matrix, out var scl, out var rot, out var pos);
            if (!decomposed)
                return false;

            position.Set(pos.X, pos.Y, pos.Z);
            rotation.Set(rot.X, rot.Y, rot.Z, rot.W);
            scale.Set(scl.X, scl.Y, scl.Z);

            return true;
        }
    }
}

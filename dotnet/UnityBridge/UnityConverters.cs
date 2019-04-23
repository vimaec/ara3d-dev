using System;
using UnityEngine;
using UnityEngine.Rendering;
using Ara3D;
// Explicitly specify math types to make it clear what each function does
using Vector3 = Ara3D.Vector3;
using Quaternion = Ara3D.Quaternion;
using Matrix4x4 = Ara3D.Matrix4x4;
using UVector3 = UnityEngine.Vector3;
using UQuaternion = UnityEngine.Quaternion;
using UMatrix4x4 = UnityEngine.Matrix4x4;

namespace UnityBridge
{
    // TODO: add an IGeometry wrapper around "Unity"
    public static class UnityConverters
    {
        public static float FEET_TO_METERS = 0.3048006096012f;

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

        // TODO: This should be pushed into the exporter.  The world deals in metric.
        public static UVector3 PositionToUnity(float x, float y, float z)
            => new UVector3(-x, z, -y) * FEET_TO_METERS;

        public static UVector3 PositionToUnity(Vector3 pos)
            => PositionToUnity(pos.X, pos.Y, pos.Z);

        public static UQuaternion RotationToUnity(Quaternion rot)
            => new UQuaternion(rot.X, -rot.Z, rot.Y, rot.W);

        public static UVector3 ScaleToUnity(Vector3 scl)
            => new UVector3(scl.X, scl.Z, scl.Y);

        public static Bounds ToUnity(this Box box)
            => new Bounds(PositionToUnity(box.Center), ScaleToUnity(box.Extent));

        public static Mesh ToUnity(this IGeometry g)
        {
            var r = new Mesh();
            
            var indices = g.IndexAttribute.ToInts();
            var vertexFloats = g.VertexAttribute.ToFloats();
            var unityVertices = vertexFloats.SelectTriplets(PositionToUnity);
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

        public static void SetFromMatrix(this Transform transform, Matrix4x4 matrix) {
            var decomposed = Ara3D.Matrix4x4.Decompose(matrix, out var scl, out var rot, out var pos);
            if (!decomposed)
                throw new Exception("Can't decompose matrix");

            ToUnityCoords(pos, rot, scl, out var outPos, out var outRot, out var outScl);

            transform.position = outPos;
            transform.rotation = outRot;
            transform.localScale = outScl;
        }

        /// <summary>
        /// Converts the given Revit-based coordinates into Unity coordinates.
        /// </summary>
        public static void ToUnityCoords(
            Vector3 pos,
            Quaternion rot,
            Vector3 scale,
            out UVector3 outPos,
            out UQuaternion outRot,
            out UVector3 outScale)
        {
            // Transform space is mirrored on X, and then rotated 90 degrees around X
            outPos = PositionToUnity(pos);

            // Quaternion is mirrored the same way, but then negated via W = -W because that's just easier to read
            outRot = RotationToUnity(rot);

            // TODO: test this, current scale is completely untested
            outScale = ScaleToUnity(scale);
        }
    }
}

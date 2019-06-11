using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Ara3D;
// Explicitly specify math types to make it clear what each function does
using Vector2 = Ara3D.Vector2;
using Vector3 = Ara3D.Vector3;
using Quaternion = Ara3D.Quaternion;
using Matrix4x4 = Ara3D.Matrix4x4;
using UVector3 = UnityEngine.Vector3;
using UVector2 = UnityEngine.Vector2;
using UQuaternion = UnityEngine.Quaternion;
using UMatrix4x4 = UnityEngine.Matrix4x4;

namespace Ara3D
{
    // TODO: add an IGeometry wrapper around "Unity"
    public static class UnityConverters
    {
        public const float FEET_TO_METERS = (float)Constants.FeetToMm * 1000f;

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
        public static UVector3 ToUnity(this Vector3 v)
            => new UVector3(v.X, v.Y, v.Z);

        public static UVector2 ToUnity(this Vector2 v)
            => new UVector2(v.X, v.Y);

        public static UVector3 PositionToUnity(float x, float y, float z)
            => new UVector3(-x, z, -y) * FEET_TO_METERS;

        public static UVector3 PositionToUnity(Vector3 pos)
            => PositionToUnity(pos.X, pos.Y, pos.Z);

        public static UQuaternion RotationToUnity(Quaternion rot)
            => new UQuaternion(rot.X, -rot.Z, rot.Y, rot.W);

        public static UVector3 SwizzleToUnity(float x, float y, float z)
            => new UVector3(x, z, y);

        public static UVector3 SwizzleToUnity(Vector3 v)
            => SwizzleToUnity(v.X, v.Z, v.Y);

        public static UVector3 ScaleToUnity(Vector3 scl)
            => SwizzleToUnity(scl);

        public static Bounds ToUnity(this AABox box)
            => new Bounds(PositionToUnity(box.Center), SwizzleToUnity(box.Extent) * FEET_TO_METERS);

        public static UVector3[] ToUnity(this IArray<Vector3> vertices)
            => vertices.Select(ToUnity).ToArray();

        public static Mesh UpdateMeshVertices(this Mesh mesh, IArray<Vector3> vertices)
        {
            mesh.vertices = vertices.ToUnity();
            return mesh;
        }

        public static MeshTopology FaceSizeToMeshTopology(int faceSize)
        {
            switch (faceSize)
            {
                case 1: return MeshTopology.Points;
                case 2: return MeshTopology.Lines;
                case 3: return MeshTopology.Triangles;
                case 4: return MeshTopology.Quads;
            }
            throw new Exception("Unsupported mesh topology");
        }

        public static Mesh UpdateMeshIndices(this Mesh mesh, IArray<int> indices, int faceSize)
        {
            switch (faceSize)
            {
                case 1: return mesh.UpdateMeshPoints(indices);
                case 2: return mesh.UpdateMeshLines(indices);
                case 3: return mesh.UpdateMeshTriangleIndices(indices);
                case 4: return mesh.UpdateMeshQuadIndices(indices);
            }
            throw new Exception("Only face sizes of 1 to 4 are supported");
        }

        public static Mesh UpdateMeshTriangleIndices(this Mesh mesh, IArray<int> triangleIndices)
        {
            if (triangleIndices.Count % 3 != 0)
                throw new Exception("Triangle index buffer must have a count divisible by 3");
            mesh.SetIndices(triangleIndices.MapIndices(TriFaceToUnity).ToArray(), MeshTopology.Triangles, 0);
            return mesh;
        }

        public static Mesh UpdateMeshQuadIndices(this Mesh mesh, IArray<int> quadIndices)
        {
            if (quadIndices.Count % 4 != 0)
                throw new Exception("Quad index buffer must have a count divisible by 4");
            mesh.SetIndices(quadIndices.MapIndices(QuadFaceToUnity).ToArray(), MeshTopology.Quads, 0);
            return mesh;
        }

        public static Mesh UpdateMeshLines(this Mesh mesh, IArray<int> lineIndices)
        {
            if (lineIndices.Count % 2 != 0)
                throw new Exception("Line index buffer must have a count divisible by 2");
            mesh.SetIndices(lineIndices.ToArray(), MeshTopology.Lines, 0);
            return mesh;
        }

        public static Mesh UpdateMeshPoints(this Mesh mesh, IArray<int> pointIndices)
        {
            mesh.SetIndices(pointIndices.ToArray(), MeshTopology.Points, 0);
            return mesh;
        }

        public static Mesh UpdateMesh(this Mesh mesh, IArray<Vector3> vertices, IArray<int> indices, int pointsPerFace)
        {
            mesh.Clear(false);
            mesh.indexFormat = vertices.Count > ushort.MaxValue
                ? IndexFormat.UInt32
                : IndexFormat.UInt16;
            mesh.UpdateMeshVertices(vertices);
            mesh.UpdateMeshIndices(indices, pointsPerFace);
            mesh.RecalculateNormals();
            return mesh;
        }

        public static Mesh UpdateMesh(this Mesh mesh, IGeometry g)
        {
            if (mesh == null || g == null)
                return mesh;

            if (g.PointsPerFace < 1 || g.PointsPerFace > 4)
                g = g.ToTriMesh();

            return mesh.UpdateMesh(g.Vertices, g.Indices, g.PointsPerFace);
            
            // TODO: copy colors, normals, uvs1 through 8, tangents, and boneWeights
            //r.colors = g.VertexColors.Select();
            //r.normals = g.VertexNormals.Select();
            // r.uv(8) = g.UV.Select();
            //r.tangents;
            //https://docs.unity3d.com/ScriptReference/BoneWeight.html
            // r.boneWeights
            //return r;
        }

        public static Mesh GetMesh(this MeshFilter filter)
            => filter == null ? null : filter.sharedMesh;

        public static Mesh GetMesh(this GameObject obj)
            => obj == null ? null : obj.GetComponent<MeshFilter>().GetMesh();

        public static Mesh UpdateMesh(this GameObject obj, IGeometry g)
            => obj == null ? null : UpdateMesh(obj.GetMesh(), g);

        public static Mesh CreateMesh(this MonoBehaviour mono, bool renderable = true)
        {
            if (mono == null || mono.gameObject == null)
                return null;
            if (renderable)
                mono.gameObject.AddComponent<MeshRenderer>();
            return mono.gameObject.AddComponent<MeshFilter>().mesh = new Mesh();
        }

        public static Mesh GetMesh(this MonoBehaviour mono)
            => mono.GetComponent<MeshFilter>().GetMesh();

        public static Mesh UpdateMesh(this MonoBehaviour mono, IGeometry g)
            => UpdateMesh(mono.GetMesh(), g);

        public static Mesh ToUnity(this IGeometry g)
            => UpdateMesh(new Mesh(), g);

        public static void SetFromNode(this Transform transform, ISceneNode node)
            => transform.SetFromMatrix(node.Transform);

        public static void SetFromMatrix(this Transform transform, Matrix4x4 matrix) {
            var decomposed = Matrix4x4.Decompose(matrix, out var scl, out var rot, out var pos);
            if (!decomposed)
                throw new Exception("Can't decompose matrix");
            transform.position = PositionToUnity(pos);
            transform.rotation = RotationToUnity(rot);
            transform.localScale = ScaleToUnity(scl);
        }
    }
}

using System.Numerics;
using System.IO;

namespace Ara3D
{
    public class RevitMeshData : IBinarySerializable
    {
        public Vector2[] uvs;
        public Vector3[] vertices;
        public Vector3[] normals;
        public int[] indices;
        public int[] normalIndices;

        public void Write(BinaryWriter bw)
        {
            bw.WriteStructs(uvs);
            bw.WriteStructs(vertices);
            bw.WriteStructs(normals);
            bw.WriteStructs(indices);
            bw.WriteStructs(normalIndices);
        }

        public void Read(BinaryReader br)
        {
            uvs = br.ReadStructs<Vector2>();
            vertices = br.ReadStructs<Vector3>();
            normals = br.ReadStructs<Vector3>();
            indices = br.ReadStructs<int>();
            normalIndices = br.ReadStructs<int>();
        }
    }

    public struct RevitNode
    {
        public Matrix4x4 WorldTransform;
        public int ElementId;
        public int FaceId;
        public int GeometryId;
    }

    public class RevitScene : IBinarySerializable
    {
        public BasicMesh[] Meshes;
        public RevitNode[] Nodes;

        public void Read(BinaryReader br)
        {
            Meshes = br.ReadClasses<BasicMesh>();
            Nodes = br.ReadStructs<RevitNode>();
        }

        public void Write(BinaryWriter bw)
        {
            bw.WriteClasses(Meshes);
            bw.WriteStructs(Nodes);
        }

        /*
        public static RevitScene Read(string file)
        {
            return new RevitScene()
        }

        public void Write(string file)
        {
            using (var fs = File.OpenWrite(file))
            using (var bw = new BinaryWriter(fs))
            {
                bw.WriteClasses(Meshes);
                bw.WriteStructs(Nodes);
            }
        }
        */
    }
}

using System.Numerics;
using System.IO;

namespace Ara3D
{
    public class BasicMesh : IBinarySerializable
    {
        public Vector3[] vertices;
        public int[] indices;

        public void Write(BinaryWriter bw)
        {
            bw.WriteStructs(vertices);
            bw.WriteStructs(indices);
        }

        public void Read(BinaryReader br)
        {
            vertices = br.ReadStructs<Vector3>();
            indices = br.ReadStructs<int>();
        }
    }
}

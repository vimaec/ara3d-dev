using g3;
using System.Linq;
using System.Numerics;

namespace Ara3D
{
    public static class G3SharpBridge
    {
        public static IArray<Vector3> ToVectors(this DVector<double> self)
        {
            return (self.Length / 3).Select(i => new Vector3((float)self[i * 3], (float)self[i * 3 + 1], (float)self[i * 3 + 2]));
        }

        public static Vector3 ToNumerics(this Vector3d self)
        {
            return new Vector3((float)self.x, (float)self.y, (float)self.z);
        }

        public static IGeometry ToIGeometry(this DMesh3 self)
        {
            self.CompactInPlace();
            var verts = self.Vertices().Select(ToNumerics).ToIArray();
            var indices = self.TrianglesBuffer.ToIArray();
            return Geometry.TriMesh(verts, indices);
        }

        public static Vector3d ToVector3D(this Vector3 self)
        {
            return new Vector3d(self.X, self.Y, self.Z);
        }

        public static Vector3d ToG3(this Vector3 self)
        {
            return new Vector3d(self.X, self.Y, self.Z);
        }

        public static DMesh3 ToG3(this IGeometry self)
        {
            var r = new DMesh3();
            foreach (var v in self.Vertices.ToEnumerable())
                r.AppendVertex(v.ToVector3D());
            var indices = self.ToTriMesh().Indices;
            for (var i = 0; i < indices.Count; i += 3)
                r.AppendTriangle(i, i + 1, i + 2);
            return r;
        }

        public static IGeometry Reduce(this IGeometry self, float percent)
        {
            return self.ToG3().Reduce(percent).ToIGeometry();
        }
    }
}

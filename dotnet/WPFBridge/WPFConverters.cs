using Ara3D;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using Geometry = Ara3D.Geometry;

namespace WPFBridge
{
    public static class WPFConverters
    {
        public static Matrix4x4 ToMatrix(this Transform3D transform)
            => transform.Value.ToMatrix4x4();

        public static Matrix4x4 ToMatrix4x4(this Matrix3D t)
            => new Matrix4x4(
                (float)t.M11, (float)t.M12, (float)t.M13, (float)t.M14,
                (float)t.M21, (float)t.M22, (float)t.M23, (float)t.M24,
                (float)t.M31, (float)t.M32, (float)t.M33, (float)t.M34,
                (float)t.OffsetX, (float)t.OffsetY, (float)t.OffsetZ, (float)t.M44);

        public static IGeometry ToIGeometry(this Model3DGroup group)
            => group.ToIGeometries(Matrix3D.Identity).ToIArray().Merge();

        public static IEnumerable<IGeometry> ToIGeometries(this Model3DGroup group, Matrix3D matrix)
        {
            matrix = matrix * group.Transform.Value;
            foreach (var model in group.Children)
            {
                var tmp = model.ToIGeometry(matrix);
                if (tmp != null)
                    yield return tmp;
            }
        }

        public static IGeometry ToIGeometry(this Model3D model, Matrix3D matrix)
            => model is GeometryModel3D gm3d
                ? gm3d.ToIGeometry(matrix)
                : null;

        public static IGeometry ToIGeometry(this GeometryModel3D model, Matrix3D matrix)
            => (model.Geometry is MeshGeometry3D mg3d)
                ? mg3d.ToIGeometry(matrix * model.Transform.Value)
                : null;

        public static Vector3 ToVector(this Point3D point)
            => new Vector3((float)point.X, (float)point.Y, (float)point.Z);

        public static IArray<Vector3> ToIArray(this Point3DCollection points)
            => points.Count.Select(i => points[i].ToVector());

        // TODO: support Geometry 
        // mesh.Normals;
        // mesh.TextureCoordinates;
        // mesh.Normals
        public static IGeometry ToIGeometry(this MeshGeometry3D mesh, Matrix3D transform)
            => Geometry.TriMesh(mesh.Positions.ToIArray(), mesh.TriangleIndices.ToIArray()).Transform(transform.ToMatrix4x4());
    }
}

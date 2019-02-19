using Ara3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace WPFBridge
{
    public static class WPFConverters
    {
        //public static Vector3 ToAra3D(Vector3 v

        // TODO: finish meee!
        /*
        public static IScene ToIGeometry(this Model3DGroup group)
        {
            foreach (var g in group.Children)
            {
                if (g is Model3DGroup)
                {

                }
            }
        }

        
        public static IGeometry ToIGeometry(this Model3D model, Transform3D t = )
        {
            var t = model.Transform;
            if (model is Geometry.)
        }*/

        public static Vector3 ToVector(this Point3D point)
            => new Vector3((float)point.X, (float)point.Y, (float)point.Z);

        public static IArray<Vector3> ToIArray(this Point3DCollection points)
            => points.Count.Select(i => points[i].ToVector());        

        public static IGeometry ToIGeometry(this MeshGeometry3D mesh)
        {
            return Geometry.TriMesh(mesh.Positions.ToIArray(), mesh.TriangleIndices.ToIArray()); 
            // TODO: support Geometry 
            // mesh.Normals;
            // mesh.TextureCoordinates;
            // mesh.Normals
            
        }
    }

}

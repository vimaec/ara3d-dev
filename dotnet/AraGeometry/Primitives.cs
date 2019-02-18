using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Ara3D
{
    public static class Primitives
    {
        public static IGeometry ToGeometry(this Box box)
            => Geometry.QuadMesh(box.Corners.ToIArray(), LinqArray.Create(
            // front 
            0, 1, 2, 3,
            // back
            5, 4, 7, 6,
            // top
            5, 4, 1, 0,
            // bottom
            3, 2, 7, 6,
            // left
            4, 0, 3, 7,
            // right
            1, 5, 6, 2));
        
        public static readonly IGeometry Cube
            = Box.Unit.ToGeometry();

        public static float Sqrt2 = 2.0f.Sqrt();

        public static readonly IGeometry Tetrahedron
            = Geometry.TriMesh(LinqArray.Create(
                    new Vector3(1f, 0.0f, -1f / Sqrt2),
                    new Vector3(-1f, 0.0f, -1f / Sqrt2),
                    new Vector3(0.0f, 1f, 1f / Sqrt2),
                    new Vector3(0.0f, -1f, 1f / Sqrt2)),
                LinqArray.Create(0, 1, 2, 1, 0, 3, 0, 2, 3, 1, 3, 2));
    }
}

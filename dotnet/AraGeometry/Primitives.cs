namespace Ara3D
{
    public static class Primitives
    {
        public static IGeometry ToGeometry(this Box box)
            => box.Corners.ToIArray().QuadMesh(LinqArray.Create(
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
            = LinqArray.Create(
                new Vector3(1f, 0.0f, -1f / Sqrt2),
                new Vector3(-1f, 0.0f, -1f / Sqrt2),
                new Vector3(0.0f, 1f, 1f / Sqrt2),
                new Vector3(0.0f, -1f, 1f / Sqrt2)).TriMesh(LinqArray.Create(0, 1, 2, 1, 0, 3, 0, 2, 3, 1, 3, 2));
    }
}

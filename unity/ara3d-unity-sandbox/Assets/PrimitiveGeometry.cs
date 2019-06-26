using Ara3D;

public class PrimitiveGeometry : ProceduralGeometry
{
    public enum GeometryType
    {
        Square,
        Cube,
        Tetrahedron
    }

    public GeometryType Type = GeometryType.Square;

    public override IGeometry Geometry
    {
        get
        {
            switch (Type)
            {
                case GeometryType.Square: return Primitives.Square;
                case GeometryType.Cube: return Primitives.Cube;
                case GeometryType.Tetrahedron: return Primitives.Tetrahedron;
            }
            return null;
        }
    }
}

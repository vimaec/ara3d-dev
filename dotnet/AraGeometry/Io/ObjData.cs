using System.Numerics;

namespace Ara3D
{
    public struct ObjGeometry 
    {
        public string Name { get; }
        public ObjMaterial Material { get; }
        public IGeometry Geometry { get; }

        public ObjGeometry(IGeometry g, string name = "")
            : this(g, name, ObjEmitter.DefaultMaterial)
        {  }

        public ObjGeometry(IGeometry g, string name, ObjMaterial mat)
        {
            Material = mat;
            Geometry = g;
            Name = name;
        }
    }

    public struct ObjMaterial
    {
        public string Name { get; }
        public Vector4 Color { get; }

        public ObjMaterial(string name, Vector4 color)
        {
            Name = name;
            Color = color;
        }

        public override bool Equals(object obj)
        {
            return obj is ObjMaterial mat && mat.Name == Name && mat.Color.Equals(Color);
        }

        public override int GetHashCode()
        {
            return Hash.Combine(Name.GetHashCode(), Color.GetHashCode());
        }
    }
}

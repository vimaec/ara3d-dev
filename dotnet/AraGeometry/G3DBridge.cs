using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Ara3D
{
    public class G3DAdapter : IGeometry
    {
        public IG3D G3D;

        public G3DAdapter(IG3D g3D)
        {
            G3D = g3D;
            var faceSizes = FaceSizeAttribute?.ToInts() ?? 3.Repeat(1);
            PointsPerFace = faceSizes.Count == 1 ? faceSizes[0] : 0;
            Vertices = VertexAttribute.ToVector3s();
            Indices = IndexAttribute?.ToInts() ?? Vertices.Indices();

            if (PointsPerFace == 0)
                NumFaces = faceSizes.Count;
            else
            {
                NumFaces = Indices.Count / PointsPerFace;
                if (Indices.Count % PointsPerFace != 0)
                    throw new Exception("The index buffer length is not a multiple of the points per face");
            }

            FaceSizes = faceSizes.Count > 1 ? faceSizes : PointsPerFace.Repeat(NumFaces);

            FaceIndices = FaceIndexAttribute?.ToInts() 
                ?? (PointsPerFace > 0
                    ? Indices.Stride(PointsPerFace)
                    : FaceSizes.PartialSums());
        }

        public int NumFaces { get; }
        public int PointsPerFace { get; }
        public IArray<Vector3> Vertices { get; }
        public IArray<int> Indices { get; }

        public IArray<int> FaceSizes { get; }
        public IArray<int> FaceIndices { get; }

        public IAttribute VertexAttribute => G3D.VertexAttribute;
        public IAttribute IndexAttribute => G3D.IndexAttribute;
        public IAttribute FaceSizeAttribute => G3D.FaceSizeAttribute;
        public IAttribute FaceIndexAttribute => G3D.FaceIndexAttribute;
        public IEnumerable<IAttribute> Attributes => G3D.Attributes;
    }

    public static class G3DBridge
    {
        public static IGeometry ToIGeometry(this IEnumerable<IAttribute> attributes)
            => attributes.ToG3D().ToIGeometry();

        public static IGeometry ToIGeometry(this IG3D g)
            => new G3DAdapter(g);

        public static IG3D ToG3D(this IGeometry g)
            => g.Attributes.ToG3D();
    }
}

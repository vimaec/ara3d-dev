using System.Collections.Generic;
using System.Numerics;

namespace Ara3D
{
    public class G3DAdapter : IGeometry
    {
        public IG3D G3D;

        public G3DAdapter(IG3D g3D)
        {
            G3D = g3D;

            FaceSizes = g3D.FaceSizes();
            PointsPerFace = g3D.HasFixedFaceSize() ? g3D.FirstFaceSize() : 0;
            Vertices = g3D.VertexAttribute.ToVector3s();
            Indices = g3D.CornerVertexIndices();
            NumFaces = g3D.FaceCount();
            
            // FaceIndices might have to be computed.
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
}

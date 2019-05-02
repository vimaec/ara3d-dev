using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ara3D
{
    [DebuggerTypeProxy(typeof(GeometryDebugView))]
    public class G3DAdapter : IGeometry
    {
        public IG3D G3D;

        public G3DAdapter(IG3D g3D)
        {
            //g3D.Validate();
            
            G3D = g3D;

            FaceSizes = g3D.FaceSizes();
            PointsPerFace = g3D.HasFixedFaceSize() ? g3D.FirstFaceSize() : 0;
            Vertices = g3D.VertexAttribute.ToVector3s();
            Indices = g3D.CornerVertexIndices();
            NumFaces = g3D.FaceCount();
            FaceIndices = g3D.FaceIndices();
            MaterialIds = g3D.MaterialIds();

            var uvAttr = g3D.UVAttributes().FirstOrDefault();
            if (uvAttr == null)
            {
                UVs = Vector2.Zero.Repeat(Vertices.Count);
            }
            else
            {
                if (uvAttr.Descriptor.Association != Association.assoc_vertex)
                    throw new Exception("The UVs must be associated with vertex");

                // TODO: I am confused by the count of the attribute
                //if (uvAttr.ElementCount() != Vertices.Count)
                //    throw new Exception("UV length is not the same as the number of vertices");
                    
                if (uvAttr.Descriptor.DataArity == 2)
                    UVs = uvAttr.ToVector2s();
                else if (uvAttr.Descriptor.DataArity == 3)
                    UVs = uvAttr.ToVector3s().Select(uv => new Vector2(uv.X, uv.Y));
                else
                    throw new Exception("UV attribute must be Vector2 or Vector3");
            }
        }

        public int NumFaces { get; }
        public int PointsPerFace { get; }
        public IArray<Vector3> Vertices { get; }
        public IArray<Vector2> UVs { get; }
        public IArray<int> Indices { get; }
        public IArray<int> FaceSizes { get; }
        public IArray<int> FaceIndices { get; }
        public IArray<int> MaterialIds { get; }

        public IAttribute VertexAttribute => G3D.VertexAttribute;
        public IAttribute IndexAttribute => G3D.IndexAttribute;
        public IAttribute FaceSizeAttribute => G3D.FaceSizeAttribute;
        public IAttribute FaceIndexAttribute => G3D.FaceIndexAttribute;
        public IAttribute MaterialIdAttribute => G3D.MaterialIdAttribute;
        public IAttribute UVAttribute => G3D.UVAttributes().FirstOrDefault();
        public IEnumerable<IAttribute> Attributes => G3D.Attributes;
    }
}

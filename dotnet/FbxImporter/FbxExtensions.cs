using FbxClrWrapper;
using System.Collections.Generic;

namespace Ara3D
{
    public static class FbxExtentions
    {
        public static G3D ToG3D(this FBXMeshData mesh)
            => new G3D(mesh.mFaceSize.ToFaceSizeAttribute(Association.assoc_face), mesh.mVertices.ToVertexAttribute(), mesh.mIndices.ToIndexAttribute());

        public static IGeometry ToIGeometry(this FBXMeshData mesh)
            => mesh.ToG3D().ToIGeometry();

        public static IArray<IGeometry> CreateIGeometyArray(this IEnumerable<FBXMeshData> meshes)
            => meshes.ToIArray().Select(ToIGeometry);
    }
}

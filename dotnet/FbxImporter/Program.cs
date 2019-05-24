using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FbxImporter
{
    class Program
    {
        static Ara3D.IArray<Ara3D.IGeometry> CreateIGeometyArray(FbxClrWrapper.FBXMeshData_ []MeshData)
        {
            Ara3D.IGeometry[] geometryArray = new Ara3D.IGeometry[MeshData.Length];

            foreach (FbxClrWrapper.FBXMeshData_ mesh in MeshData)
            {
                Ara3D.IAttribute faceSizeAttribute = Ara3D.G3DExtensions.ToFaceSizeAttribute(mesh.mFaceSize);
                Ara3D.IAttribute vertexAttribute = Ara3D.G3DExtensions.ToVertexAttribute(mesh.mVertices);
                Ara3D.IAttribute indexAttribute = Ara3D.G3DExtensions.ToIndexAttribute(mesh.mIndices);

                IEnumerable<Ara3D.IAttribute> attributeList = new List<Ara3D.IAttribute>{ faceSizeAttribute, vertexAttribute, indexAttribute };
                Ara3D.G3D g3d = new Ara3D.G3D(attributeList);
            }

            return new Ara3D.FunctionalArray<Ara3D.IGeometry>(geometryArray.Length, n => geometryArray[n]);
        }

        static void Main(string[] args)
        {
            FbxClrWrapper.FBXLoader.Initialize();
      //      FbxClrWrapper.FBXLoader.LoadFBX("D:/MyDev/ozz-animation-0.10.0/media/fbx/baked.fbx");
            FbxClrWrapper.FBXLoader.LoadFBX("E:/VimAecDev/vims/Models/MobilityPavilion_mdl.fbx");
            //                   FbxClrWrapper.FBXLoader.LoadFBX("E:/VimAecDev/vims/Models/CDiggins_313401_S_v19.fbx");


            FbxClrWrapper.FBXSceneData_ sceneData = FbxClrWrapper.FBXLoader.GetSceneData();

            Ara3D.IArray<Ara3D.IGeometry> geometryArray = CreateIGeometyArray(sceneData.mMeshList);

        //    Ara3D.IScene scene = new Ara3D.Scene(rootNode, geometryArray, nodeArray);

            FbxClrWrapper.FBXLoader.ShutDown();
        }
    }
}

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

            for (int i = 0; i < MeshData.Length; i++)
            {
                FbxClrWrapper.FBXMeshData_ mesh = MeshData[i];

                Ara3D.IAttribute faceSizeAttribute = Ara3D.G3DExtensions.ToFaceSizeAttribute(mesh.mFaceSize);
                Ara3D.IAttribute vertexAttribute = Ara3D.G3DExtensions.ToVertexAttribute(mesh.mVertices);
                Ara3D.IAttribute indexAttribute = Ara3D.G3DExtensions.ToIndexAttribute(mesh.mIndices);

                IEnumerable<Ara3D.IAttribute> attributeList = new List<Ara3D.IAttribute>{ faceSizeAttribute, vertexAttribute, indexAttribute };
                Ara3D.G3D g3d = new Ara3D.G3D(attributeList);

                geometryArray[i] = Ara3D.Geometry.ToIGeometry(g3d);
            }

            return new Ara3D.FunctionalArray<Ara3D.IGeometry>(geometryArray.Length, n => geometryArray[n]);
        }

        static Ara3D.IArray<Ara3D.ISceneNode> CreateISceneNodeArray(FbxClrWrapper.FBXSceneData_ SceneData, Ara3D.IArray<Ara3D.IGeometry> GeometryArray)
        {
            Ara3D.ISceneNode[] nodeArray = new Ara3D.ISceneNode[SceneData.mNodeNameList.Length];

            for (int i = 0; i < SceneData.mNodeNameList.Length; i++)
            {
                Ara3D.IGeometry geomtry = null;
                if (SceneData.mNodeMeshIndexList[i] != -1)
                {
                    geomtry = GeometryArray[SceneData.mNodeMeshIndexList[i]];
                }

                Ara3D.Vector3 translation = new Ara3D.Vector3(
                    SceneData.mNodeTranslationList[i * 3 + 0],
                    SceneData.mNodeTranslationList[i * 3 + 1],
                    SceneData.mNodeTranslationList[i * 3 + 2]);

                Ara3D.Vector3 scale = new Ara3D.Vector3(
                    SceneData.mNodeScaleList[i * 3 + 0],
                    SceneData.mNodeScaleList[i * 3 + 1],
                    SceneData.mNodeScaleList[i * 3 + 2]);

                Ara3D.Vector3 rotation = new Ara3D.Vector3(
                    SceneData.mNodeRotationList[i * 3 + 0],
                    SceneData.mNodeRotationList[i * 3 + 1],
                    SceneData.mNodeRotationList[i * 3 + 2]);

                Ara3D.Matrix4x4 translationMatrix = Ara3D.Matrix4x4.CreateTranslation(translation);
                Ara3D.Matrix4x4 scaleMatrix = Ara3D.Matrix4x4.CreateScale(scale);
                Ara3D.Matrix4x4 rotationMatrix = Ara3D.Matrix4x4.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);

                nodeArray[i] = new Ara3D.SceneNode(geomtry, Ara3D.Matrix4x4.Multiply(Ara3D.Matrix4x4.Multiply(translationMatrix, rotationMatrix), scaleMatrix));
            }

            for (int i = 0; i < SceneData.mNodeNameList.Length; i++)
            {
                int parentIndex = SceneData.mNodeParentList[i];

                if (parentIndex != -1)
                {
                    Ara3D.SceneNode parentNode = nodeArray[parentIndex] as Ara3D.SceneNode;
                    Ara3D.SceneNode node = nodeArray[i] as Ara3D.SceneNode;
                    parentNode._AddChild(node);
                }
            }

            return new Ara3D.FunctionalArray<Ara3D.ISceneNode>(nodeArray.Length, n => nodeArray[n]);
        }

        static void Main(string[] args)
        {
            FbxClrWrapper.FBXLoader.Initialize();
            FbxClrWrapper.FBXLoader.LoadFBX("E:/VimAecDev/vims/Models/MobilityPavilion_mdl.fbx");
            //FbxClrWrapper.FBXLoader.LoadFBX("E:/VimAecDev/vims/Models/CDiggins_313401_S_v19.fbx");

            FbxClrWrapper.FBXSceneData_ sceneData = FbxClrWrapper.FBXLoader.GetSceneData();

            Ara3D.IArray<Ara3D.IGeometry> geometryArray = CreateIGeometyArray(sceneData.mMeshList);
            Ara3D.IArray<Ara3D.ISceneNode> sceneNodeArray = CreateISceneNodeArray(sceneData, geometryArray);

            Ara3D.IScene scene = new Ara3D.Scene(sceneNodeArray[0], geometryArray, sceneNodeArray);

            FbxClrWrapper.FBXLoader.ShutDown();
        }
    }
}

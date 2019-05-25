using Ara3D;
using FbxClrWrapper;
using System.Collections.Generic;

namespace Ara3D.FbxImporter
{
    public static class Program
    {
        public static G3D ToG3D(this FBXMeshData_ mesh)
            => new G3D(mesh.mFaceSize.ToFaceSizeAttribute(), mesh.mVertices.ToVertexAttribute(), mesh.mIndices.ToIndexAttribute());

        public static IGeometry ToIGeometry(this FBXMeshData_ mesh)
            => mesh.ToG3D().ToIGeometry();

        public static IArray<IGeometry> CreateIGeometyArray(this IEnumerable<FBXMeshData_> meshes)
            => meshes.ToIArray().Select(ToIGeometry);

        public static IArray<ISceneNode> CreateISceneNodeArray(FBXSceneData_ sceneData, IArray<IGeometry> geometries)
        {
            var nodeArray = new ISceneNode[sceneData.mNodeNameList.Length];

            for (var i = 0; i < sceneData.mNodeNameList.Length; i++)
            {
                var id = sceneData.mNodeMeshIndexList[i];
                var geometry = id >= 0 ? geometries[id] : null;
                
                var translation = new Vector3(
                    sceneData.mNodeTranslationList[i * 3 + 0],
                    sceneData.mNodeTranslationList[i * 3 + 1],
                    sceneData.mNodeTranslationList[i * 3 + 2]);

                var scale = new Vector3(
                    sceneData.mNodeScaleList[i * 3 + 0],
                    sceneData.mNodeScaleList[i * 3 + 1],
                    sceneData.mNodeScaleList[i * 3 + 2]);

                var rotation = new Vector3(
                    sceneData.mNodeRotationList[i * 3 + 0],
                    sceneData.mNodeRotationList[i * 3 + 1],
                    sceneData.mNodeRotationList[i * 3 + 2]);

                var translationMatrix = Matrix4x4.CreateTranslation(translation);
                var scaleMatrix = Matrix4x4.CreateScale(scale);

                // TODO: this is different from the description of argument of the CreateFromYawPitchRoll function (which could be wrong)
                var rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);

                nodeArray[i] = new SceneNode(geometry, translationMatrix * rotationMatrix * scaleMatrix);
            }

            for (var i = 0; i < sceneData.mNodeNameList.Length; i++)
            {
                var parentIndex = sceneData.mNodeParentList[i];

                if (parentIndex != -1)
                {
                    var parentNode = nodeArray[parentIndex] as SceneNode;
                    var node = nodeArray[i] as SceneNode;
                    parentNode._AddChild(node);
                }
            }

            return nodeArray.ToIArray();
        }

        public static void Main(string[] args)
        {
            FBXLoader.Initialize();
            //var filePath = @"E:/VimAecDev/vims/Models/MobilityPavilion_mdl.fbx";
            var filePath = @"C:\Users\ASUS\AppData\Local\Temp\ara3d\output\JobFbxExport\10042-MDL-C134003-BM-000001\10042-MDL-C134003-BM-000001.fbx";
            FBXLoader.LoadFBX(filePath);
            //FbxClrWrapper.FBXLoader.LoadFBX("E:/VimAecDev/vims/Models/CDiggins_313401_S_v19.fbx");

            var sceneData = FBXLoader.GetSceneData();

            var geometryArray = CreateIGeometyArray(sceneData.mMeshList);
            var sceneNodeArray = CreateISceneNodeArray(sceneData, geometryArray);

            var scene = new Scene(sceneNodeArray[0], geometryArray, sceneNodeArray);
            var outputFilePath = @"C:\Users\ASUS\AppData\Local\Temp\ara3d\output\test.obj";
            scene.ToIGeometry().WriteObj(outputFilePath);

            // TODO: Does this need to happen after everything, if not perhaps it should happen as soon as the load happens. 
            // maybe the whole thing could be done using the IDisposable pattern. 
            FBXLoader.ShutDown();
        }
    }
}

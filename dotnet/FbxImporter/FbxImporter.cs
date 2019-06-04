using FbxClrWrapper;
using System.Collections.Generic;

namespace Ara3D
{
    public static class FbxImporter
    {
        public static G3D ToG3D(this FBXMeshData mesh)
            => new G3D(mesh.mFaceSize.ToFaceSizeAttribute(), mesh.mVertices.ToVertexAttribute(), mesh.mIndices.ToIndexAttribute());

        public static IGeometry ToIGeometry(this FBXMeshData mesh)
            => mesh.ToG3D().ToIGeometry();

        public static IArray<IGeometry> CreateIGeometyArray(this IEnumerable<FBXMeshData> meshes)
            => meshes.ToIArray().Select(ToIGeometry);

        public static IArray<ISceneNode> CreateISceneNodeArray(FBXSceneData sceneData, IArray<IGeometry> geometries)
        {
            var nodeArray = new ISceneNode[sceneData.mNodeNameList.Length];

            for (var i = 0; i < sceneData.mNodeNameList.Length; i++)
            {
                var id = sceneData.mNodeMeshIndexList[i];
                var geometry = id >= 0 ? geometries[id] : null;

            /*    var translation = new Vector3(
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

                var rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);*/

                var transformMatrix = new Matrix4x4(
                    sceneData.mNodeTransformList[i * 16 + 0 + 0],
                    sceneData.mNodeTransformList[i * 16 + 1 + 0],
                    sceneData.mNodeTransformList[i * 16 + 2 + 0],
                    sceneData.mNodeTransformList[i * 16 + 3 + 0],
                    sceneData.mNodeTransformList[i * 16 + 0 + 4],
                    sceneData.mNodeTransformList[i * 16 + 1 + 4],
                    sceneData.mNodeTransformList[i * 16 + 2 + 4],
                    sceneData.mNodeTransformList[i * 16 + 3 + 4],
                    sceneData.mNodeTransformList[i * 16 + 0 + 8],
                    sceneData.mNodeTransformList[i * 16 + 1 + 8],
                    sceneData.mNodeTransformList[i * 16 + 2 + 8],
                    sceneData.mNodeTransformList[i * 16 + 3 + 8],
                    sceneData.mNodeTransformList[i * 16 + 0 + 12],
                    sceneData.mNodeTransformList[i * 16 + 1 + 12],
                    sceneData.mNodeTransformList[i * 16 + 2 + 12],
                    sceneData.mNodeTransformList[i * 16 + 3 + 12]
                    );

                nodeArray[i] = new SceneNode(geometry, sceneData.mNodeNameList[i], transformMatrix);
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

        public static IScene CreateScene(string FBXFileName)
        {
            FBXLoader.Initialize();
            if (FBXLoader.LoadFBX(FBXFileName) >= 0)
            {
                FBXLoader.ShutDown();

                var sceneData = FBXLoader.GetSceneData();

                ValidateSceneData(sceneData);

                var geometryArray = CreateIGeometyArray(sceneData.mMeshList);
                var sceneNodeArray = CreateISceneNodeArray(sceneData, geometryArray);

                var scene = new Scene(sceneNodeArray[0], geometryArray, sceneNodeArray);
                return scene;
            }

            FBXLoader.ShutDown();
            return null;
        }

        public static void CreateFBX(IScene Scene, string FBXFileName)
        {
            FBXLoader.Initialize();
            if (FBXLoader.SaveFBX(FBXFileName) >= 0)
            {
                FBXLoader.ShutDown();

                var sceneData = FBXLoader.GetSceneData();

                ValidateSceneData(sceneData);

                var geometryArray = CreateIGeometyArray(sceneData.mMeshList);
                var sceneNodeArray = CreateISceneNodeArray(sceneData, geometryArray);

                var scene = new Scene(sceneNodeArray[0], geometryArray, sceneNodeArray);
                return;
            }

            FBXLoader.ShutDown();
            return;
        }

        public static bool ValidateSceneData(FBXSceneData SceneData)
        {
            int totalFaces = 0;
            int totalTriangles = 0;
            int totalIndices = 0;
            foreach (var mesh in SceneData.mMeshList)
            {
                int meshIndices = 0;
                foreach (var faceSize in mesh.mFaceSize)
                {
                    meshIndices += faceSize;
                    totalTriangles += faceSize + 1 - 3;
                }

                if (meshIndices != mesh.mIndices.Length)
                {
                    return false;
                }

                totalIndices += meshIndices;
                totalFaces += mesh.mFaceSize.Length;
            }

            return true;
        }
    }
}

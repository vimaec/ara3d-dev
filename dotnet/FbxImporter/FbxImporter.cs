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

                nodeArray[i] = new SceneNode(geometry, sceneData.mNodeNameList[i], translationMatrix * rotationMatrix * scaleMatrix);
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
                // TODO: Does this need to happen after everything, if not perhaps it should happen as soon as the load happens. 
                // maybe the whole thing could be done using the IDisposable pattern. 
                FBXLoader.ShutDown();

                var sceneData = FBXLoader.GetSceneData();

                var geometryArray = CreateIGeometyArray(sceneData.mMeshList);
                var sceneNodeArray = CreateISceneNodeArray(sceneData, geometryArray);

                var scene = new Scene(sceneNodeArray[0], geometryArray, sceneNodeArray);
                return scene;
            }

            FBXLoader.ShutDown();
            return null;
        }
    }
}

using FbxClrWrapper;
using System.Collections.Generic;

namespace Ara3D
{

    public class FbxImporter
    {
        FbxCliReader fbxReader = new FbxCliReader();

        public IArray<ISceneNode> CreateISceneNodeArray(FBXSceneData sceneData, IArray<IGeometry> geometries)
        {
            var nodeArray = new ISceneNode[sceneData.mNodeNameList.Length];

            for (var i = 0; i < sceneData.mNodeNameList.Length; i++)
            {
                var id = sceneData.mNodeMeshIndexList[i];
                var geometry = id >= 0 ? geometries[id] : null;

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

                var properties = new Properties(
                    new Dictionary<string, string>
                    {
                        {"name", sceneData.mNodeNameList[i]},
                    });
                nodeArray[i] = new SceneNode(properties, geometry, transformMatrix);
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

        public IScene LoadFBX(string FBXFileName)
        {
            fbxReader.Initialize();
            if (fbxReader.LoadFBX(FBXFileName) >= 0)
            {
                var sceneData = fbxReader.GetSceneData();
                fbxReader.ShutDownAPI();

                var geometryArray = sceneData.mMeshList.CreateIGeometyArray();
                var sceneNodeArray = CreateISceneNodeArray(sceneData, geometryArray);

                var scene = new Scene(new SceneProperties(), sceneNodeArray[0]);
                return scene;
            }

            fbxReader.ShutDownAPI();
            throw new System.Exception("Failed to load FBX File");
        }
    }
}

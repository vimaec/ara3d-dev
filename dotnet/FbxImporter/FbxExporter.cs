using FbxClrWrapper;
using System.Collections.Generic;

namespace Ara3D
{

    public class FbxExporter
    {
        FbxCliWriter fbxWriter = new FbxCliWriter();
        public void DestroyData()
        {
            fbxWriter.DestroyData();
        }

        private void CreateFBXNodesAndMeshes(IEnumerable<ISceneNode> Nodes, ref FBXSceneData SceneData)
        {
            var nodeNameList = new List<string>();
            var nodeParentList = new List<int>();
            var nodeTransformList = new List<float>();
            var nodeMeshIndexList = new List<int>();
            var meshList = new List<FBXMeshData>();
            var meshIdList = new List<string>();

            // For performance and practicality, we need to generate a map from node/geom to index in the array
            var sceneNodeMap = new Dictionary<ISceneNode, int>();
            var geomertryMap = new Dictionary<IGeometry, int>();

            int nodeIndex = 0;
            foreach (var node in Nodes)
            {
                sceneNodeMap[node] = nodeIndex++;
            }

            foreach (var node in Nodes)
            {
                nodeNameList.Add(node.Properties["name"]);
                nodeParentList.Add(node.Parent != null ? sceneNodeMap[node.Parent] : -1);
                if (node.Geometry != null)
                {
                    if (geomertryMap.ContainsKey(node.Geometry))
                    {
                        nodeMeshIndexList.Add(geomertryMap[node.Geometry]);
                    }
                    else
                    {
                        var mesh = new FBXMeshData();

                        mesh.mIndicesAttribute = node.Geometry.IndexAttribute;
                        mesh.mVerticesAttribute = node.Geometry.VertexAttribute;
                        mesh.mFaceSizeAttribute = node.Geometry.FaceSizeAttribute;

                        var meshIndex = meshList.Count;

                        meshList.Add(mesh);
                        meshIdList.Add("<no name>");

                        geomertryMap[node.Geometry] = meshIndex;
                        nodeMeshIndexList.Add(meshIndex);
                    }
                }
                else
                {
                    nodeMeshIndexList.Add(-1);
                }

                nodeTransformList.AddRange(node.Transform.ToFloats());
            }

            SceneData.mNodeNameList = nodeNameList.ToArray();
            SceneData.mNodeParentList = nodeParentList.ToArray();
            SceneData.mNodeTransformList = nodeTransformList.ToArray();
            SceneData.mNodeMeshIndexList = nodeMeshIndexList.ToArray();
            SceneData.mMeshList = meshList.ToArray();
            SceneData.mMeshIdList = meshIdList.ToArray();
        }

        public void SaveFBX(IScene Scene, string FBXFileName)
        {
            fbxWriter.Initialize();

            var sceneData = new FBXSceneData();
//            CreateFBXMeshList(Scene.Geometries, ref sceneData);
//            CreateFBXNodes(Scene.Nodes, Scene.Geometries, ref sceneData);
            CreateFBXNodesAndMeshes(Scene.AllNodes(), ref sceneData);
            fbxWriter.SetSceneData(sceneData);

            if (fbxWriter.SaveFBX(FBXFileName) >= 0)
            {
                fbxWriter.ShutDownAPI();
                return;
            }

            fbxWriter.ShutDownAPI();
            throw new System.Exception("Failed to save FBX File");
        }
    }
}

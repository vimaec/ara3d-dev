using FbxClrWrapper;
using System.Collections.Generic;

namespace Ara3D
{
    public static class FbxImporter
    {
        public static G3D ToG3D(this FBXMeshData mesh)
            => new G3D(mesh.mFaceSize.ToFaceSizeAttribute(Association.assoc_face), mesh.mVertices.ToVertexAttribute(), mesh.mIndices.ToIndexAttribute());

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

        private static void CreateFBXMeshList(IArray<IGeometry> Geometries, ref FBXSceneData SceneData)
        {
            var meshList = new List<FBXMeshData>();
            var meshIdList = new List<string>();

            for (int geometryIndex = 0; geometryIndex < Geometries.Count; geometryIndex++)
            {
                var mesh = new FBXMeshData();
                var geometry = Geometries[geometryIndex];

                mesh.mInds = geometry.Indices;
                mesh.mVerts = geometry.Vertices.ToFloats();
                mesh.mFcSz = geometry.FaceSizes;

                meshList.Add(mesh);
                meshIdList.Add("<no name>");
            }

            SceneData.mMeshList = meshList.ToArray();
            SceneData.mMeshIdList = meshIdList.ToArray();
        }
        
        private static void CreateFBXNodes(IArray<ISceneNode> Nodes, IArray<IGeometry> Geometries, ref FBXSceneData SceneData)
        {
            var nodeNameList = new List<string>();
            var nodeParentList = new List<int>();
            var nodeTransformList = new List<float>();
            var nodeMeshIndexList = new List<int>();

            // For performance and practicality, we need to generate a map from node/geom to index in the array
            var sceneNodeMap = new Dictionary<ISceneNode, int>();
            var geomertryMap = new Dictionary<IGeometry, int>();

            for (int nodeIndex = 0; nodeIndex < Nodes.Count; nodeIndex++)
            {
                sceneNodeMap[Nodes[nodeIndex]] = nodeIndex;
            }

            for (int geometryIndex = 0; geometryIndex < Geometries.Count; geometryIndex++)
            {
                geomertryMap[Geometries[geometryIndex]] = geometryIndex;
            }

            for (int nodeIndex = 0; nodeIndex < Nodes.Count; nodeIndex++)
            {
                var node = Nodes[nodeIndex];
                nodeNameList.Add(node.Properties["name"]);
                nodeParentList.Add(node.Parent != null ? sceneNodeMap[node.Parent] : -1);
                nodeMeshIndexList.Add((node.Geometry != null && geomertryMap.ContainsKey(node.Geometry)) ? geomertryMap[node.Geometry] : -1);
                nodeTransformList.AddRange(node.Transform.ToFloats());
            }

            SceneData.mNodeNameList = nodeNameList.ToArray();
            SceneData.mNodeParentList = nodeParentList.ToArray();
            SceneData.mNodeTransformList = nodeTransformList.ToArray();
            SceneData.mNodeMeshIndexList = nodeMeshIndexList.ToArray();
        }

        private static void CreateFBXNodesAndMeshes(IEnumerable<ISceneNode> Nodes, ref FBXSceneData SceneData)
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

                        mesh.mInds = node.Geometry.Indices;
                        mesh.mVerts = node.Geometry.Vertices.ToFloats();
                        mesh.mFcSz = node.Geometry.FaceSizes;

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

        public static IScene LoadFBX(string FBXFileName)
        {
            FBXLoader.Initialize();
            if (FBXLoader.LoadFBX(FBXFileName) >= 0)
            {
                var sceneData = FBXLoader.GetSceneData();
                FBXLoader.ShutDown();

                var geometryArray = CreateIGeometyArray(sceneData.mMeshList);
                var sceneNodeArray = CreateISceneNodeArray(sceneData, geometryArray);

                var scene = new Scene(new SceneProperties(), sceneNodeArray[0]);
                return scene;
            }

            FBXLoader.ShutDown();
            throw new System.Exception("Failed to load FBX File");
        }

        public static void SaveFBX(IScene Scene, string FBXFileName)
        {
            FBXLoader.Initialize();

            var sceneData = new FBXSceneData();
//            CreateFBXMeshList(Scene.Geometries, ref sceneData);
//            CreateFBXNodes(Scene.Nodes, Scene.Geometries, ref sceneData);
            CreateFBXNodesAndMeshes(Scene.AllNodes(), ref sceneData);
            FBXLoader.SetSceneData(sceneData);

            if (FBXLoader.SaveFBX(FBXFileName) >= 0)
            {
                FBXLoader.ShutDown();
                return;
            }

            FBXLoader.ShutDown();
            throw new System.Exception("Failed to save FBX File");
        }
    }
}

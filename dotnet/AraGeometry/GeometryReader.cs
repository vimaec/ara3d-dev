using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ara3D
{
    public static class GeometryReader
    {
        public static IGeometry ReadG3D(string filePath)
            => G3DExtensions.ReadFromFile(filePath).ToIGeometry();

        public static IList<ManifestSceneNode> LoadManifest(string filePath) 
           => Util.LoadJsonArray(filePath).ToObject<IList<ManifestSceneNode>>();

        /// <summary>
        /// Loads a scene from a folder containing G3D files and a manifest. 
        /// </summary>
        public static IScene ReadScene(string folder)
        {
            var manifestFile = Path.Combine(folder, "manifest.json");
            if (!File.Exists(manifestFile))
                throw new Exception($"Could not find manifest.json file in folder {folder}");

            var nodes = LoadManifest(manifestFile);
            var files = Directory.GetFiles(folder, @"*.g3d");

            // Load the G3D files in parallel and put in a lookup table
            var geometries = files.AsParallel().ToDictionary(
                f => int.Parse(Path.GetFileNameWithoutExtension(f) ?? throw new InvalidOperationException()),
                ReadG3D);

            // Check that all expected geometries are present (0..n-1)
            for (var i=0; i < geometries.Count; ++i)
                if (!geometries.ContainsKey(i))
                    throw new Exception($"Could not find expected geometry index {i}");

            // Check that all referenced geometries in the manifest are present 
            foreach (var n in nodes)
                if (!geometries.ContainsKey(n.GeometryId))
                    throw new Exception($"Could not find geometry {n.GeometryId}.g3d in folder {folder}");
            
            // Create an array of scene nodes 
            var sceneNodes = nodes.Select(
                node => new SceneNode(node.ElementId.ToString(), node.GeometryId, node.Transform.ToMatrix(),
                    node.MaterialId, node.CategoryId) as ISceneNode);

            // Get the geometries in order 
            var orderedGeometries = geometries.OrderBy(kv => kv.Key).Select(kv => kv.Value);

            // Create the scene 
            return new Scene(orderedGeometries.ToIArray(), sceneNodes.ToIArray());
        }
    }
}

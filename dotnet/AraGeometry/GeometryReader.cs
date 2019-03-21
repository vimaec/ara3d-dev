using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Ara3D
{
    public static class GeometryReader
    {
        public static IGeometry ReadG3D(string filePath)
            => G3DExtensions.ReadFromFile(filePath).ToIGeometry();

        public static List<IGeometry> LoadGeometriesFromBFast(string filePath)
            => BFastExtensions.ReadBFast(filePath).LoadGeometries();

        public static List<IGeometry> LoadGeometries(this BFast bfast)
            => bfast.Buffers
                .AsParallel().AsOrdered()
                .Select(b => G3D.Create(b).ToIGeometry())
                .ToList();

        /*
        public static IList<ManifestSceneNode> ReadManifest(string filePath) 
           => Util.LoadJsonArray(filePath).ToObject<IList<ManifestSceneNode>>();

        public static IArray<ISceneNode> ManifestNodesToSceneNodes(this IList<ManifestSceneNode> manifest)
            => manifest.Select(
                node => new SceneNode(node.ElementId.ToString(), node.GeometryId, node.Transform.ToMatrix(),
                    node.ElementId, node.MaterialId) as ISceneNode).ToIArray();

        /// <summary>
        /// Loads a scene from a folder containing G3D files and a manifest. 
        /// </summary>
        public static IScene ReadScene(string folder)
        {
            var manifestFile = Path.Combine(folder, "manifest.json");
            if (!File.Exists(manifestFile))
                throw new Exception($"Could not find manifest.json file in folder {folder}");

            var nodes = ReadManifest(manifestFile);
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
                      
            // Get the geometries in order 
            var orderedGeometries = geometries.OrderBy(kv => kv.Key).Select(kv => kv.Value);

            // Create the scene 
            return new Scene(orderedGeometries.ToIArray(), nodes.ManifestNodesToSceneNodes());
        }

        /// <summary>
        /// Loads a scene embedded in a BFAST package.
        /// </summary>
        public static IScene ReadSceneFromBFast(string filePath)
        {
            var bFast = BFastExtensions.ReadBFast(filePath);

            var manifestBuffer = bFast.Buffers[0];

            // TODO: converting to a string from a span would be more efficient
            var manifestText = manifestBuffer.ToBytes().ToUtf8();

            // TODO: the G3D create here is doing a copy under the hood, which sucks
            var geometries = bFast.Buffers.Skip(1).AsParallel().AsOrdered().Select(b => G3D.Create(b).ToIGeometry()).ToList();

            // Get the scene nodes from the manifest 
            var manifest = JArray.Parse(manifestText).ToObject<IList<ManifestSceneNode>>();

            // Creates a scene API
            return new Scene(geometries.ToIArray(), manifest.ManifestNodesToSceneNodes());
        }
        */

        public static IScene ReadScene(string folder)
            => throw new NotImplementedException("See commented out code");

        public static IScene ReadSceneFromBFast(string filePath)
            => throw new NotImplementedException("See commented out code");
    }
}

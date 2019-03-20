using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ara3D.DotNetUtilities;
using Newtonsoft.Json.Linq;

namespace Ara3D
{
    public static class GeometryReader
    {
        public static IGeometry ReadG3D(string filePath)
            => G3DExtensions.ReadFromFile(filePath).ToIGeometry();

        public static IList<ManifestSceneNode> ReadManifest(string manifestJson)
           => JArray.Parse(manifestJson).ToObject<IList<ManifestSceneNode>>();

        public static IArray<ISceneNode> ManifestNodesToSceneNodes(this IList<ManifestSceneNode> manifest)
            => manifest.Select(
                node => new SceneNode(node.ElementId.ToString(), node.GeometryId, node.Transform.ToMatrix(),
                    node.ElementId, node.MaterialId, node.CategoryId) as ISceneNode).ToIArray();

        /// <summary>
        /// Loads a scene from a folder containing G3D files and a manifest.
        /// Enumerator returns an IScene on load completion
        /// </summary>
        public static async Task<IScene> ReadScene(IFormatLoader loader)
        {
            // We wait while the loader gets us our resources
            var data = await loader.ResourceManifestAsync();
            // Our load is lazy, which means we only load the bare
            // minimum by default.  Further loads (eg geometry, BIM data)
            // will trigger the fetching of the data required
            return data != null ? new Scene(ReadManifest(data).ManifestNodesToSceneNodes(), loader) : null;
        }

        // A BFast is just a special type of resource loader
        /// <summary>
        /// Loads a scene embedded in a BFAST package.
        /// </summary>
        //public static IScene ReadSceneFromBFast(string filePath)
        //{
        //    var bFast = BFastExtensions.ReadBFast(filePath);

        //    var manifestBuffer = bFast.Buffers[0];

        //    // TODO: converting to a string from a span would be more efficient
        //    var manifestText = manifestBuffer.ToBytes().ToUtf8();

        //    // TODO: the G3D create here is doing a copy under the hood, which sucks
        //    var geometries = bFast.Buffers.Skip(1).AsParallel().AsOrdered().Select(b => G3D.Create(b).ToIGeometry()).ToList();

        //    // Get the scene nodes from the manifest 
        //    var manifest = JArray.Parse(manifestText).ToObject<IList<ManifestSceneNode>>();

        //    // Creates a scene API
        //    throw new ApplicationException();
        //    //return new Scene(manifest.ManifestNodesToSceneNodes(), null);
        //}
    }
}

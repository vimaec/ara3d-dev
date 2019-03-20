using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Ara3D.Tests
{
    [TestFixture]
    public static class InstancedTests
    {
        public static string InputFolderEnvVariable = "ARA3D_TEST_INPUT_PATH";
        public static string InputFolder = Environment.GetEnvironmentVariable(InputFolderEnvVariable);
        public static string OutputFolder = Path.Combine(Path.GetTempPath(), "ara3d", "test_output");

        [SetUp]
        public static void SetUp()
        {            
            if (string.IsNullOrEmpty(InputFolder))
                throw new Exception($"Could not find environment variable {InputFolderEnvVariable}.");
            if (!Directory.Exists(InputFolder))
                throw new Exception($"No input folder could be found: {InputFolder}");
            Directory.CreateDirectory(OutputFolder);
        }
        
        [Test, Explicit("Hard coded paths")]
        public static void TestBFastLoading()
        {
            //var sw = new Stopwatch();
            
            ////var inputFile = @"C:\Users\ara3d\AppData\Local\Ara3D\RevitDevPlugin\2019-03-04_19-47-52\output.vim";
            //var inputFile =
            //    @"C:\Users\ara3d\AppData\Local\Ara3D\RevitDevPlugin\2019-03-04_23-12-13-main\output.vim";

            //var scene = GeometryReader.ReadSceneFromBFast(inputFile);
            //sw.OutputTimeElapsed($"Loaded file {inputFile}");

            //var d = new DictionaryOfLists<int, ISceneObject>();
            //foreach (var obj in scene.Objects.ToEnumerable())
            //    d.Add(obj.Node.GeometryId, obj);

            //var uninstancedObjects = d.Where(kv => kv.Value.Count == 1).Select(kv => kv.Value[0]).ToList();
            //Console.WriteLine($"Total object count = {scene.Objects.Count}");
            //Console.WriteLine($"Uninstanced object count = {uninstancedObjects.Count}");

            //var unreferencedObjectsCount = scene.Geometries.Indices().CountWhere(i => !d.ContainsKey(i));
            //Console.WriteLine($"Unreferenced object count = {unreferencedObjectsCount}");

            //var emptyGeos = scene.Geometries.CountWhere(g => g.NumFaces == 0);
            //Console.WriteLine($"Empty geometry count = {emptyGeos}");
        }

        [Test]
        public static void TestBFastToObj()
        {
            //var inputFile =
            //    @"C:\Users\ara3d\AppData\Local\Ara3D\RevitDevPlugin\2019-03-05_00-47-15-rac_basic_sample_project\output.vim";
            //var scene = GeometryReader.ReadSceneFromBFast(inputFile);
            //scene.ToIGeometry().WriteObj(@"C:\Users\ara3d\AppData\Local\Ara3D\RevitDevPlugin\test.obj");
        }

        /// <summary>
        /// Generates a BFAST from a directory containing G3D files and a manifest.json
        /// </summary>
        [Test]
        public static void CreateBFast()
        {
            /*
            var sw = new Stopwatch();
            sw.Start();

            var folder = Path.Combine(InputFolder, "revit_g3d_list");

            sw.OutputTimeElapsed("Reading JSON manifest");
            var manifestFile = Path.Combine(folder, "manifest.json");
            var manifest = Util.LoadJsonArray(manifestFile);

            sw.OutputTimeElapsed("Converting JSON array to nodes");
            var nodes = manifest.ToObject<IList<ManifestSceneNode>>();

            var files = Directory.GetFiles(folder, @"*.g3d");

            sw.OutputTimeElapsed($"Loading {files.Length} G3D files");
            var geometries = files.AsParallel().ToDictionary(
                f => int.Parse(Path.GetFileNameWithoutExtension(f)),
                File.ReadAllBytes);

            sw.OutputTimeElapsed("Remapping geometry indices in manifest");
            var geometryIndexLookup = new Dictionary<int, int>();
            var buffers = new List<byte[]>();

            var n = 0; 
            foreach (var kv in geometries)
            {
                geometryIndexLookup[kv.Key] = n++;
                buffers.Add(kv.Value);
            }

            // Remap the indices 
            foreach (var node in nodes)
                node. = geometryIndexLookup[node.Geometry];

            sw.OutputTimeElapsed("Creating a new manifest");
            var newManifest = nodes.ToJArray().ToString();
            buffers.Insert(0, newManifest.ToBytesUtf8());

            sw.OutputTimeElapsed("Generating BFAST buffer");
            var bfast = buffers.ToBFast();

            sw.OutputTimeElapsed("Writing the bfast");
            var outputFile = Path.Combine(OutputFolder, "instanced.bfast");
            bfast.WriteToFile(outputFile);
            sw.OutputTimeElapsed($"Finished writing BFast to {outputFile}");

            sw.OutputTimeElapsed("Reading BFast and generating OBJ");
            ReadBFastAndGenerateObj(outputFile);
            */
        }

        /*
        public static void ReadBFastAndGenerateObj(string filePath)
        {
            // TODO: I don't like ReadBFast on BFastExtensions it is not discoverable
            var bFast = BFastExtensions.ReadBFast(filePath);
            var manifestBuffer = bFast.Buffers[0];

            // TODO: converting to a string from a span would be more efficient
            var manifestText = manifestBuffer.ToBytes().ToUtf8();

            // TODO: the G3D create here is doing a copy under the hood, which sucks
            var geometries = bFast.Buffers.Skip(1).AsParallel().Select(b => G3D.Create(b).ToIGeometry()).ToList();

            // Get the scene nodes from the manifest 
            var manifest = JArray.Parse(manifestText).ToObject<IList<ManifestSceneNode>>();

            // Transform all of the geometries and merge them together
            var g = manifest.Select(node => geometries[node.Geometry].Transform(node.Transform.ToMatrix())).Merge();

            // Output the merged OBJ
            var outputFile = filePath + ".obj";
            g.WriteObj(outputFile);
        }
        */
    }
}

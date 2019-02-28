using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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
        }

        [Test, Explicit("Hard coded paths")]
        public static void SecondInstanceTest()
        {
            var folder = @"c:\dev\tmp\vim-export-demo-copy2";

            // I would like to make this parallel, but Sizeof is a problem 
            var files = Directory.GetFiles(folder, @"*.g3d");
            var geometries = files.AsParallel().ToDictionary(
                f => int.Parse(Path.GetFileNameWithoutExtension(f)),
                f => G3D.ReadFile(f).ToIGeometry());                 
            var manifest = Util.LoadJsonArray(Path.Combine(folder, "manifest.json"));
            var nodes = manifest.ToObject<IList<SimpleSceneNode>>();
            var g = nodes.Select(n => geometries[n.Geometry].Transform(n.Transform.ToMatrix())).Merge();
            g.WriteG3D(@"C:\dev\tmp\instanced.g3d");
            g.WriteObj(@"C:\dev\tmp\instanced.obj");
        }

        /// <summary>
        /// Generates a BFAST from a directory containing G3D files and a manifest.json
        /// </summary>
        [Test]
        public static void CreateBFast()
        {
            var sw = new Stopwatch();
            sw.Start();

            var folder = Path.Combine(InputFolder, "revit_g3d_list");

            sw.OutputTimeElapsed("Reading JSON manifest");
            var manifestFile = Path.Combine(folder, "manifest.json");
            var manifest = Util.LoadJsonArray(manifestFile);

            sw.OutputTimeElapsed("Converting JSON array to nodes");
            var nodes = manifest.ToObject<IList<SimpleSceneNode>>();

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
                node.Geometry = geometryIndexLookup[node.Geometry];

            sw.OutputTimeElapsed("Creating a new manifest");
            var newManifest = nodes.ToJArray().ToString();
            buffers.Insert(0, File.ReadAllBytes(manifestFile));

            sw.OutputTimeElapsed("Generating BFAST buffer");
            var bfast = buffers.ToBFast();

            sw.OutputTimeElapsed("Writing the bfast");
            var outputFile = Path.Combine(OutputFolder, "instanced.bfast");
            bfast.WriteToFile(outputFile);
            sw.OutputTimeElapsed($"Finished writing BFast to {outputFile}");

            sw.OutputTimeElapsed("Reading BFast and generating OBJ");
            ReadBFastAndGenerateObj(outputFile);
        }

        public static void ReadBFastAndGenerateObj(string filePath)
        {
            // TODO: I don't like ReadBFast on BFastExtensions it is not discoverable
            var bFast = BFastExtensions.ReadBFast(filePath);
            var manifestBuffer = bFast.Buffers[0];

            // TODO: converting to a string from a span would be more effiient
            var manifestText = manifestBuffer.ToBytes().ToUtf8();

            // TODO: the G3D create here is doing a copy under the hood, which sucks
            var geometries = bFast.Buffers.Skip(1).AsParallel().Select(b => G3D.Create(b).ToIGeometry()).ToList();

            // Get the scene nodes from the manifest 
            var manifest = JArray.Parse(manifestText).ToObject<IList<SimpleSceneNode>>();

            // Transform all of the geometries and merge them together
            var g = manifest.Select(node => geometries[node.Geometry].Transform(node.Transform.ToMatrix())).Merge();

            // Output the merged OBJ
            var outputFile = filePath + ".obj";
            g.WriteObj(outputFile);

        }

        /// <summary>
        /// This tests the current system in place for supporting face groups and instances
        /// based on face groups embedded in a G3D. To be determined whether I continue supporting
        /// that. My impression is that it leaves a lot of complexity.
        /// </summary>
        [Test]
        public static void ReadInstancesFromG3DTest()
        {
            foreach (var fileName in IOTests.GetInputFiles("model.g3d"))
            {
                var g = Util.TimeIt(() => G3D.ReadFile(fileName).ToIGeometry(), $"Reading G3D {fileName}");

                g.Validate();
                TestGeometries.OutputIGeometryStats(g);

                var instanceTransforms = g.Attributes(AttributeType.attr_instance_transform).First().ToMatrices();
                var instanceGroups = g.Attributes(AttributeType.attr_instance_group).First().ToInts();
                var groupIndexes = g.Attributes(AttributeType.attr_group_index).First().ToInts();
                var groupSizes = g.Attributes(AttributeType.attr_group_size).First().ToInts();

                Assert.AreEqual(instanceTransforms.Count, instanceGroups.Count);
                Assert.AreEqual(groupIndexes.Count, groupSizes.Count);
                Assert.IsTrue(groupIndexes.All(x => x >= 0 && x < g.NumFaces));
                Assert.IsTrue(instanceGroups.All(x => x >= 0 && x < instanceTransforms.Count));

                var nFaces = 0;
                for (var i = 0; i < instanceGroups.Count; ++i)
                {
                    var size = groupSizes[i];
                    var index = groupIndexes[i];
                    Assert.IsTrue(index + size <= g.NumFaces);
                    nFaces += groupSizes[i];
                }
                Console.WriteLine($"Total faces = {nFaces}");
                
                Directory.CreateDirectory(IOTests.TestOutputFolder);
                var outputFileName = Path.Combine(IOTests.TestOutputFolder, "unmerged.obj");
                g.WriteObj(outputFileName);
                Console.WriteLine($"Wrote unmerged data to {outputFileName}");

                /*
                var groupGeometries = groupIndexes.Zip(groupSizes,
                    (i, n) => Geometry.TriMesh(g.Vertices, g.Indices.SubArray(i * 3, n * 3)).RemoveUnusedVertices()).ToArray();

                var instanceGeometries = instanceGroups.Zip(instanceTransforms,
                    (grp, t) => groupGeometries[grp].Transform(t));

                var mergedGeometry = instanceGeometries.Merge();

                Directory.CreateDirectory(IOTests.TestOutputFolder);
                outputFileName = Path.Combine(IOTests.TestOutputFolder, "merged.obj");

                mergedGeometry.WriteObj(outputFileName);
                Console.WriteLine($"Wrote merged data to {outputFileName}");
                */
            }
        }
    }
}

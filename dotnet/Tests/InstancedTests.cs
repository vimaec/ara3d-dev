using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Ara3D.Tests
{
    [TestFixture]
    public static class InstancedTests
    { 
        [Test]
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

        [Test]
        public static void FirstInstanceTest()
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

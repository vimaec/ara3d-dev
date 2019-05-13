using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using g3;
using WPFBridge;

namespace Ara3D.Tests
{
    // TODO: look at other file readers
    // * https://github.com/stefangordon/ObjParser
    // * https://github.com/chrisjansson/ObjLoader/tree/master/source/CjClutter.ObjLoader.Loader
    // * C:\dev\sdks\helix-toolkit-develop\Source\HelixToolkit.Wpf\Importers\ModelImporter.cs
    [TestFixture]
    public static class IOTests
    {
        // TODO: make this an environment variable
        public static string TestInputFolder = @"C:\dev\repos\_test_input";
        public static string TestOutputFolder = Path.Combine(Path.GetTempPath(), "ara3d", "_test_output");

        public static IEnumerable<string> GetInputFiles(string ext)
        {
            return Directory.GetFiles(TestInputFolder, ext, SearchOption.AllDirectories).OrderBy(a => a);
        }

        public static void OutputDMeshStats(DMesh3 m)
        {
            Console.WriteLine($"Vertex count {m.VertexCount}");
            Console.WriteLine($"Triangle count {m.TriangleCount}");
            Console.WriteLine($"Edge count {m.EdgeCount}");
            Console.WriteLine($"Max Vertex ID {m.MaxVertexID}");
            Console.WriteLine($"Max Triangle ID {m.MaxTriangleID}");
            Console.WriteLine($"Max Edge ID {m.MaxEdgeID}");
            Console.WriteLine($"Max Group ID {m.MaxGroupID}");
        }

        public static void CheckAttribute(IAttribute attr, Association assoc, AttributeType at, DataType dt, int arity)
        {
            Assert.AreEqual(arity, attr.Descriptor._data_arity);
            Assert.AreEqual((int) assoc, attr.Descriptor._association);
            Assert.AreEqual((int) at, attr.Descriptor._attribute_type);
            Assert.AreEqual((int) dt, attr.Descriptor._data_type);
        }

        public static void CheckStandardGeometryAttributes(IG3D g3d)
        {
            var vertexBuffer = g3d.VertexAttribute;
            var indexBuffer = g3d.IndexAttribute;
            CheckAttribute(vertexBuffer, Association.assoc_vertex, AttributeType.attr_vertex, DataType.dt_float32, 3);
            CheckAttribute(indexBuffer, Association.assoc_corner, AttributeType.attr_index, DataType.dt_int32, 1);
        }

        public static void CheckTestTetrahedron(IGeometry g3d)
        {
            CheckStandardGeometryAttributes(g3d);
            var vertices = g3d.Vertices.ToArray();
            var indices = g3d.Indices.ToArray();
            Assert.AreEqual(TestGeometries.TestTetrahedronVertices, vertices);
            Assert.AreEqual(TestGeometries.TestTetrahedronIndices, indices);
        }

        [Test]
        public static void TestWritingProceduralGeometries()
        {
            var i = 0;
            var outputFolder = Path.Combine(TestOutputFolder, "procedural");
            foreach (var g in TestGeometries.AllGeometries)
            {
                TestWritingGeometry(g, outputFolder, i.ToString());

                var g1 = G3Sharp.LoadGeometry(Path.Combine(outputFolder, $"{i}.ara.obj"));
                TestGeometries.BasicCompareGeometries(g.ToTriMesh(), g1);

                var g2 = G3Sharp.LoadGeometry(Path.Combine(outputFolder, $"{i}.g3.obj"));
                TestGeometries.BasicCompareGeometries(g.ToTriMesh(), g2);

                // STL files are odd, because they don't share vertices,
                // var g3 = G3Sharp.LoadGeometry(Path.Combine(outputFolder, $"{i}.g3.stl"));
                // TestGeometries.BasicCompareGeometries(g.ToTriMesh(), g3, true);

                i++;
            }
        }

        /// <summary>
        /// This test checks the basic construction of a G3D from attributes. 
        /// </summary>
        [Test]
        public static void SimpleCreateG3DTest()
        {
            // Should be a tetrahedron
            var vertexBuffer = TestGeometries.TestTetrahedronVertices.ToVertexAttribute();
            var indexBuffer = TestGeometries.TestTetrahedronIndices.ToIndexAttribute();
            CheckAttribute(vertexBuffer, Association.assoc_vertex, AttributeType.attr_vertex, DataType.dt_float32, 3);
            CheckAttribute(indexBuffer, Association.assoc_corner, AttributeType.attr_index, DataType.dt_int32, 1);
            var g3d = G3D.Create(vertexBuffer, indexBuffer);
            Assert.AreEqual(4, vertexBuffer.Count);
            Assert.AreEqual(12, indexBuffer.Count);
            var g = g3d.ToIGeometry();
            CheckTestTetrahedron(g);
            var b = g.ToBFast();
            var g2 = b.ToG3D();
            CheckTestTetrahedron(g2.ToIGeometry());
            var tmpPath = Path.GetTempFileName();
            g.WriteG3D(tmpPath);
            var g3 = G3DExtensions.ReadFromFile(tmpPath);
            CheckTestTetrahedron(g3.ToIGeometry());
        }

        public static void CompareDMesh(DMesh3 m, IGeometry g, double tolerance)
        {
            Assert.AreEqual(3, g.PointsPerFace);
            Assert.AreEqual(m.VertexCount, g.Vertices.Count);
            Assert.AreEqual(m.TriangleCount, g.Indices.Count / 3);
            Assert.AreEqual(0, g.Indices.Count % 3);
            var i = 0;
            foreach (var tri in m.Triangles())
            {
                Assert.AreEqual(tri.a, g.Indices[i++]);
                Assert.AreEqual(tri.b, g.Indices[i++]);
                Assert.AreEqual(tri.c, g.Indices[i++]);
            }

            i = 0;
            foreach (var v in m.Vertices())
            {
                var v2 = g.Vertices[i++];
                Assert.AreEqual(v.x, v2.X, tolerance);
                Assert.AreEqual(v.y, v2.Y, tolerance);
                Assert.AreEqual(v.z, v2.Z, tolerance);
            }
        }

        public static void WriteFileStats(string file)
        {
            var fi = new FileInfo(file);
            Console.WriteLine($"File name: {file}");
            Console.WriteLine($"Extension: {Path.GetExtension(file).ToLowerInvariant()}");
            Console.WriteLine($"File size: {fi.Length}");
            //Console.WriteLine($"File last modified: {File.GetLastWriteTime(file)}");
            //Console.WriteLine($"File last accessed: {File.GetLastAccessTime(file)}");
            Console.WriteLine($"File created: {File.GetCreationTime(file)}");
        }

        public static void TestWritingFile(string filePath, Action<string> writer)
        {
            try
            {
                var folder = Path.GetDirectoryName(filePath);
                Directory.CreateDirectory(folder);
                var sw = new Stopwatch();
                sw.Start();
                Console.WriteLine("Writing geometry to disk: {0}", filePath);
                writer(filePath);
                Assert.IsTrue(File.Exists(filePath));
                sw.Stop();
                var time = sw.ElapsedMilliseconds;
                var size = Util.FileSize(filePath);
                var speed = size / (time * 1000f);
                Console.WriteLine($"Writing speed: {speed} MB/S");
                Console.WriteLine($"Time elapsed {sw.PrettyPrintTimeElapsed()}");
                WriteFileStats(filePath);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        public static void TestWritingGeometry(IGeometry g, string outputFolder, string fileName)
        {
            // Try writing to an OBJ: using DMesh, using Helix, using my OBJ writer 
            // Try writing to a G3D
            // Time the different writing 
            // Try conversions back from the different forms. and the reading. 
            // 
            TestGeometries.OutputTriangleStatsSummary(g);
            TestGeometries.OutputIGeometryStats(g);

            var baseFileName = Path.GetFileName(fileName);
            var outputFileName = Path.Combine(outputFolder, baseFileName);

            Console.WriteLine("Testing Ara 3D native writer");
            TestWritingFile(outputFileName + ".ara.obj", f => g.WriteObj(f, false));

            var g3dFileName = outputFileName + ".ara.g3d";
            TestWritingFile(g3dFileName, g.WriteG3D);

            Console.WriteLine("Testing G3 Sharp writer");
            TestWritingFile(outputFileName + ".g3.obj", f => g.ToG3Sharp().WriteFile(f));
            TestWritingFile(outputFileName + ".g3.stl", f => g.ToG3Sharp().WriteFileBinary(f));

            // TODO: test writing using Helix

            // Check that reading the G3D back-in yields the same IGeometry
            var g2 = Ara3D.Util.TimeIt(() => G3DExtensions.ReadFromFile(g3dFileName).ToIGeometry(), "Reading time for G3D");
            TestGeometries.CompareGeometries(g, g2);
        }

        // This requires input files in a special folder 
        [Test]
        public static void TestObjReader()
        {
            foreach (var f in GetInputFiles("angel.obj"))
            {
                WriteFileStats(f);

                Console.WriteLine("HelixToolkit OBJ Reader");
                Console.WriteLine("=======================");
                var helixObjModel3DGroup = Ara3D.Util.TimeIt(
                    () => Helix.LoadFileModel3DGroup(f), "Loading OBJ with Helix");

                var gFromHelix = helixObjModel3DGroup.ToIGeometry();
                TestWritingGeometry(gFromHelix, Path.Combine(TestOutputFolder, "helix"), f);

                Console.WriteLine("geometry3Sharp OBJ Reader");
                Console.WriteLine("=========================");
                var g3SharpObjList = Ara3D.Util.TimeIt(
                    () => G3Sharp.LoadMeshes(f), $"Loading OBJ with geometry3Sharp");

                for (var i = 0; i < g3SharpObjList.Count; ++i)
                {
                    var dmesh = g3SharpObjList[i];
                    CompareDMesh(dmesh, dmesh.ToIGeometry(), TestGeometries.SmallTolerance);
                }

                var gFromG3SharpObjList = g3SharpObjList.ToIGeometry();
                TestWritingGeometry(gFromG3SharpObjList, Path.Combine(TestOutputFolder, "g3sharp"), f);
            }
        }

        // TODO: 
        // Test merge
        // Test deform, move one direction move back, is same? 
        // Compare to DMesh3 (which is a jerk) 
        // Get a torus created. 
        // Generate the tetrahedron. 


        [Test]
        public static void TestG3DReader()
        {
            foreach (var fileName in GetInputFiles("*.g3d"))
            {
                var g = Util.TimeIt(() => G3D.ReadFile(fileName), $"Reading G3D {fileName}");

                var baseFileName = Path.GetFileName(fileName);
                var outputFileName = Path.Combine(TestOutputFolder, baseFileName);

                Console.WriteLine("Testing Ara 3D native writer");
                TestWritingFile(outputFileName + ".ara.obj", f => g.ToIGeometry().WriteObj(f, false));
            }
        }
    }
}

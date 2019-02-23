using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using g3;
using WPFBridge;

namespace Ara3D.Tests
{
    // TODO: look at other file readers
    // * https://github.com/stefangordon/ObjParser
    // * https://github.com/chrisjansson/ObjLoader/tree/master/source/CjClutter.ObjLoader.Loader
    // * C:\dev\sdks\helix-toolkit-develop\Source\HelixToolkit.Wpf\Importers\ModelImporter.cs
    [TestFixture]
    public static class G3dTests
    {
        // TODO: make this an environment variable
        public static string TestInputFolder = @"C:\dev\repos\_test_input";
        public static string TestOutputFolder = @"C:\dev\repos\_test_ouput";

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

        public static void OutputIGeometryStats(IGeometry g)
        {
            var stats = g.GetStats();
            Console.WriteLine(stats);
            Assert.IsTrue(g.AreAllIndicesValid());
            // Attributes present? 
        }

        public static void G3SharpReaderTest(Func<List<g3.DMesh3>> f) {
            try
            {
                var meshes = f.TimeIt();
                if (meshes == null) Console.WriteLine("Failed to open file");
                else
                {
                    Console.WriteLine($"Success, {meshes.Count} meshes found");
                    foreach (var m in meshes)
                    {
                        OutputDMeshStats(m);

                        var g = m.ToIGeometry();
                        var stats = g.GetStats();
                        Console.WriteLine(stats);

                        // TODO: time it, trap exceptions, add an assertion
                        var tmpOBJFile = Path.ChangeExtension(Path.GetTempFileName(), ".obj");
                        g.WriteObj(tmpOBJFile);
                        
                        // TODO: time it, trap exceptions, add an assertion
                        var tmpG3DFile = Path.ChangeExtension(Path.GetTempFileName(), ".g3d");
                        g.WriteToFile(tmpG3DFile);

                        // TODO: view it (or draw it or something)

                        //var tmp = g.ToG3D();
                        //var g3 = tmp.ToIGeometry();*
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void CheckAttribute(IAttribute attr, Association assoc, AttributeType at, DataType dt, int arity)
        {
            Assert.AreEqual(arity, attr.Descriptor._data_arity);
            Assert.AreEqual((int)assoc, attr.Descriptor._association);
            Assert.AreEqual((int)at, attr.Descriptor._attribute_type);
            Assert.AreEqual((int)dt, attr.Descriptor._data_type);
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
            Assert.AreEqual(TestTetrahedronVertices, vertices);
            Assert.AreEqual(TestTetrahedronIndices, indices);
        }

        public static readonly Vector3[] TestTetrahedronVertices = { Vector3.Zero, Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ };
        public static readonly int[] TestTetrahedronIndices = { 0, 1, 2, 0, 3, 1, 1, 3, 2, 2, 3, 0 };

        /// <summary>
        /// This test checks the basic construction of a G3D from attributes. 
        /// </summary>
        [Test]    
        public static void SimpleCreateG3DTest()
        {
            // Should be a tetrahedron
            var vertexBuffer = TestTetrahedronVertices.ToVertexAttribute();
            var indexBuffer = TestTetrahedronIndices.ToIndexAttribute();
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
            g.WriteToFile(tmpPath);
            var g3 = G3DExtensions.ReadFromFile(tmpPath);
            CheckTestTetrahedron(g3.ToIGeometry());
        }

        public static void CompareMeshes(DMesh3 m, IGeometry g, double tolerance = 0.00001)
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
            Console.WriteLine($"File last modified: {File.GetLastWriteTime(file)}");
            Console.WriteLine($"File last accessed: {File.GetLastAccessTime(file)}");
            Console.WriteLine($"File created: {File.GetCreationTime(file)}");
        }

        public static void TestGeometry(IGeometry g)
        {
            // Try writing to an OBJ: using DMesh, using Helix, using my OBJ writer 
            // Try writing to a G3D
            // Time the different writing 
            // Try conversions back from the different forms. and the reading. 
            // 
            OutputIGeometryStats(g);

            var tmpOBJFile = Path.ChangeExtension(Path.GetTempFileName(), ".obj");
            Util.TimeIt(() => g.WriteObj(tmpOBJFile), $"Writing to {tmpOBJFile}");

            // TODO: time it, trap exceptions, add an assertion
            var tmpG3DFile = Path.ChangeExtension(Path.GetTempFileName(), ".g3d");
            Util.TimeIt(() => g.WriteToFile(tmpG3DFile), $"Writing to {tmpG3DFile}");
        }

        [Test, Explicit]
        public static void TestObjReaderAndWriter()
        {
            foreach (var f in GetInputFiles("*.obj"))
            {
                WriteFileStats(f);

                var helixObjModel3DGroup = Util.TimeIt(
                    () => Helix.LoadFileModel3DGroup(f), "Loading OBJ with Helix");

                var gFromHelix = helixObjModel3DGroup.ToIGeometry();
                TestGeometry(gFromHelix);

                var g3SharpObjList = Util.TimeIt(
                    () => G3Sharp.LoadGeometry(f), $"Loading OBJ with G3D sharp");

                var gFromG3SharpObjList = g3SharpObjList.Select(G3Sharp.ToIGeometry).Merge();
                TestGeometry(gFromG3SharpObjList);

                // How long does it take to write an obj using Helix bridge
                // How long does it take to write an obj using DMesh 
                // Are there invalid indices? 
                // What attributes are present? 
            }
        }

        [Test, Explicit]
        public static void G3SharpReaderTests()
        {
            Console.WriteLine("Testing G3 Sharp ");

            Console.WriteLine("Testing G3Sharp OBJ reader");           
            foreach (var f in GetInputFiles("*.obj"))
            {
                Console.WriteLine($"Opening: {f}");
                G3SharpReaderTest(() => G3Sharp.LoadGeometry(f));
            }

            Console.WriteLine("Testing G3Sharp OFF reader");
            foreach (var f in GetInputFiles("*.off"))
            {
                Console.WriteLine($"Opening: {f}");
                G3SharpReaderTest(() => G3Sharp.LoadGeometry(f));
            }

            Console.WriteLine("Testing G3Sharp STL reader");
            foreach (var f in GetInputFiles("*.stl"))
            {
                Console.WriteLine($"Opening: {f}");
                G3SharpReaderTest(() => G3Sharp.LoadGeometry(f));
            }
        }
    }
}

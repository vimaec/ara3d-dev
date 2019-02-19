using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;

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
        public static string BaseAssetFolder = @"c:\dev\assets";

        public static IEnumerable<string> GetAssetFiles(string ext)
        {
            return Directory.GetFiles(BaseAssetFolder, ext, SearchOption.AllDirectories).OrderBy(a => a);
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

        public static void CompareMeshes(DMesh3 m, IGeometry g, double tolerance = 0.00001)
        {
            Assert.AreEqual(3, g.PointsPerFace);
            Assert.AreEqual(m.VertexCount, g.Vertices.Count);
            Assert.AreEqual(m.TriangleCount, g.Indices.Count);
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

        [Test, Explicit]
        public static void TestObjReaderAndWriter()
        {
            var baseFolder = @"..\..\..\..\testdata";
            if (!Directory.Exists(baseFolder))
                throw new Exception("Could not find input folder for test data");
            var path = @"C:\dev\assets\obj\angel.obj";
            var meshes = G3SharpIO.ReadGeometry(path);
            var mesh = meshes[0];
            CompareMeshes(mesh, mesh.ToIGeometry());

            // TODO: try writing the damn thing out, and reading it 
        }

        [Test, Explicit]
        public static void G3SharpReaderTests()
        {
            Console.WriteLine("Testing G3 Sharp ");

            Console.WriteLine("Testing G3Sharp OBJ reader");           
            foreach (var f in GetAssetFiles("*.obj"))
            {
                Console.WriteLine($"Opening: {f}");
                G3SharpReaderTest(() => G3SharpIO.ReadGeometry(f));
            }

            Console.WriteLine("Testing G3Sharp OFF reader");
            foreach (var f in GetAssetFiles("*.off"))
            {
                Console.WriteLine($"Opening: {f}");
                G3SharpReaderTest(() => G3SharpIO.ReadGeometry(f));
            }

            Console.WriteLine("Testing G3Sharp STL reader");
            foreach (var f in GetAssetFiles("*.stl"))
            {
                Console.WriteLine($"Opening: {f}");
                G3SharpReaderTest(() => G3SharpIO.ReadGeometry(f));
            }
        }
    }
}

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ara3D.Tests
{
    // TODO: look at other file readers
    // * https://github.com/stefangordon/ObjParser
    // * https://github.com/chrisjansson/ObjLoader/tree/master/source/CjClutter.ObjLoader.Loader
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

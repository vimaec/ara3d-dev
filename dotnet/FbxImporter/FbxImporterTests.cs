using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Ara3D
{
    [TestFixture]
    class FbxImporterTests
    {
        [TestCase(@"E:\VimAecDev\vims\Models\MobilityPavilion_mdl.fbx", @"d:\test.obj")]
        //[TestCase(@"D:\DATA\10042-MDL-C134003-BM-000001.fbx", @"D:\DATA\out_1.obj")]
        //[TestCase(@"D:\DATA\C14 - 92001 Sustainability Pavillion Master.fbx", @"D:\DATA\out_2.obj")] // NOTE: this one ran out of memory.
        public static void TestFbxImporter(string filePath, string outputFilePath)
        {
            var scene = FbxImporter.CreateScene(filePath);
            scene.ToIGeometry().WriteObj(outputFilePath);
        }
    }
}

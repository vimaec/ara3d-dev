using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Ara3D
{
    [TestFixture]
    public class FbxImporterTests
    {
        public static readonly Vector3[] TestTetrahedronVertices = { Vector3.Zero, Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ };
        public static readonly int[] TestTetrahedronIndices = { 0, 1, 2, 0, 3, 1, 1, 3, 2, 2, 3, 0 };
        public static IGeometry Tetrahedron = TestTetrahedronVertices.ToIArray().TriMesh(TestTetrahedronIndices.ToIArray());

        [TestCase(@"E:\VimAecDev\vims\Models\MobilityPavilion_mdl.fbx", @"d:\test.obj")]
        //[TestCase(@"D:\DATA\10042-MDL-C134003-BM-000001.fbx", @"D:\DATA\out_1.obj")]
        //[TestCase(@"D:\DATA\C14 - 92001 Sustainability Pavillion Master.fbx", @"D:\DATA\out_2.obj")] // NOTE: this one ran out of memory.
        public static void TestFbxImporter(string filePath, string outputFilePath)
        {
            //            var scene = FbxImporter.LoadFBX(filePath);
            //            scene.ToIGeometry().WriteObj(outputFilePath);

            var scene = CreateTextIScene();
            FbxImporter.SaveFBX(scene, @"test.fbx");
            var scene2 = FbxImporter.LoadFBX(@"test.fbx");

            Assert.IsTrue(scene == scene2);
        }

        public static Properties CreateNodeProperties(string Name)
        {
            return new Properties(
            new Dictionary<string, string>
            {
                {"name", Name},
            });
        }

        public static IScene CreateTextIScene()
        {
            Vector3 vertex0 = new Vector3(-50, 0, 50);
            Vector3 vertex1 = new Vector3(50, 0, 50);
            Vector3 vertex2 = new Vector3(50, 100, 50);
            Vector3 vertex3 = new Vector3(-50, 100, 50);
            Vector3 vertex4 = new Vector3(-50, 0, -50);
            Vector3 vertex5 = new Vector3(50, 0, -50);
            Vector3 vertex6 = new Vector3(50, 100, -50);
            Vector3 vertex7 = new Vector3(-50, 100, -50);

            var rootNode = new SceneNode(CreateNodeProperties("RootNode"), null, Matrix4x4.Identity);
            var child1 = new SceneNode(CreateNodeProperties("Child1"), Tetrahedron, Matrix4x4.CreateTranslation(new Vector3(100, 100, 100)));
            var child2 = new SceneNode(CreateNodeProperties("Child2"), Tetrahedron, Matrix4x4.CreateTranslation(new Vector3(-100, -100, -100)));

            rootNode._AddChild(child1);
            rootNode._AddChild(child2);

            return new Scene(new SceneProperties(), rootNode);
        }
    }
}

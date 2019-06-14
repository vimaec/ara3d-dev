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
            FbxExporter exporter = new FbxExporter();
            exporter.SaveFBX(scene, @"test.fbx");

            FbxImporter importer = new FbxImporter();
            var scene2 = importer.LoadFBX(@"test.fbx");

            CompareScenes(scene, scene2);
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

        public static void CompareScenes(IScene Scene1, IScene Scene2)
        {
            CompareNodes(Scene1.Root, Scene2.Root);
        }

        public static void CompareNodes(ISceneNode Node1, ISceneNode Node2)
        {
            Assert.IsTrue(Node1.GetName() == Node2.GetName());
            Assert.IsTrue(Node1.Children.Count == Node2.Children.Count);

            var children1 = Node1.Children.ToArray();
            var children2 = Node2.Children.ToArray();

            Array.Sort(children1, (x, y) => x.GetName().CompareTo(y.GetName()));
            Array.Sort(children2, (x, y) => x.GetName().CompareTo(y.GetName()));

            for (int i = 0; i < children1.Length; i++)
            {
                CompareNodes(children1[i], children2[i]);
                CompareGeometries(Node1.Geometry, Node2.Geometry);
            }
        }

        public static void CompareGeometries(IGeometry Geometry1, IGeometry Geometry2)
        {
            if (Geometry1 == null || Geometry2 == null)
            {
                Assert.IsTrue(Geometry1 == null && Geometry2 == null);
                return;
            }

            Assert.IsTrue(Geometry1.Indices.Count == Geometry2.Indices.Count);
            Assert.IsTrue(Geometry1.FaceSizes.Count == Geometry2.FaceSizes.Count);
            Assert.IsTrue(Geometry1.Vertices.Count == Geometry2.Vertices.Count);
            
            for (int i = 0; i < Geometry1.Indices.Count; i++)
            {
                Assert.IsTrue(Geometry1.Indices[i] == Geometry2.Indices[i]);
            }

            for (int i = 0; i < Geometry1.Vertices.Count; i++)
            {
                Assert.IsTrue(Geometry1.Vertices[i] == Geometry2.Vertices[i]);
            }

            for (int i = 0; i < Geometry1.FaceSizes.Count; i++)
            {
                Assert.IsTrue(Geometry1.FaceSizes[i] == Geometry2.FaceSizes[i]);
            }
        }
    }
}

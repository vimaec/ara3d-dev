using System;
using System.Linq;
using NUnit.Framework;

namespace Ara3D
{
    [TestFixture]
    public static class TestGeometries
    {
        public static IGeometry XYTriangle = new[] { new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1f, 0f, 0f) }.ToIArray().TriMesh(3.Range());
        public static IGeometry XYQuad = new[] { new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(1f, 1f, 0f), new Vector3(1f, 0f, 0f) }.ToIArray().QuadMesh(4.Range());
        public static IGeometry XYQuadFromFunc = Geometry.QuadMesh(uv => uv.ToVector3(), 1, 1);
        public static IGeometry XYQuad2x2 = Geometry.QuadMesh(uv => uv.ToVector3(), 2, 2);
        public static IGeometry XYTriangleTwice = XYTriangle.Merge(XYTriangle.Translate(new Vector3(1, 0, 0)));

        public static readonly Vector3[] TestTetrahedronVertices = { Vector3.Zero, Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ };
        public static readonly int[] TestTetrahedronIndices = { 0, 1, 2, 0, 3, 1, 1, 3, 2, 2, 3, 0 };

        public static IGeometry Tetrahedron =
            TestTetrahedronVertices.ToIArray().TriMesh(TestTetrahedronIndices.ToIArray());

        public static IGeometry Torus = Geometry.QuadMesh(uv => TorusFunction(uv, 10, 0.2f), 10, 24);

        static IGeometry RevolvedVerticalCylinder(float height, float radius, int verticalSegments, int radialSegments)
            => (Vector3.UnitZ * height).ToLine().Interpolate(verticalSegments).Add(-radius.AlongX()).RevolveAroundAxis(Vector3.UnitZ, radialSegments);

        public static IGeometry Cylinder = RevolvedVerticalCylinder(5, 1, 4, 12);

        public static IGeometry[] AllGeometries = {
            XYTriangle, // 0
            XYQuad, // 1
            XYQuadFromFunc, // 2
            XYQuad2x2, // 3
            Tetrahedron, // 4
            Torus, // 5
            Cylinder, // 6
            XYTriangleTwice, // 7
        };

        public static double SmallTolerance = 0.0001;

        // https://github.com/mrdoob/three.js/blob/master/src/geometries/ParametricGeometry.js
        // https://github.com/mrdoob/three.js/blob/master/src/geometries/TorusGeometry.js

        public static Vector3 TorusFunction(Vector2 uv, float radius, float tube)
        {
            uv = uv * Constants.TwoPi;
            return new Vector3(
                (radius + tube * uv.Y.Cos()) * uv.X.Cos(),
                (radius + tube * uv.Y.Cos()) * uv.X.Sin(),
                tube * uv.X.Sin());
        }

        public static void BasicCompareGeometries(IGeometry g1, IGeometry g2)
        {
            Assert.AreEqual(g1.NumFaces, g2.NumFaces);
            Assert.AreEqual(g1.Vertices.Count, g2.Vertices.Count);
            Assert.AreEqual(g1.Indices.Count, g2.Indices.Count);

            Assert.IsTrue(g1.Indices.SequenceEquals(g2.Indices));

            for (int i = 0; i < g1.Vertices.Count; i++)
            {
                var v1 = g1.Vertices[i];
                var v2 = g2.Vertices[i];
                Assert.IsTrue(v1.AlmostEquals(v2, (float)SmallTolerance));
            }
        }

        public static void CompareGeometries(IGeometry g1, IGeometry g2)
        {
            BasicCompareGeometries(g1, g2);

            Assert.AreEqual(g1.Attributes.Count(), g2.Attributes.Count());
            var attrs1 = g1.SortedAttributes().ToList();
            var attrs2 = g2.SortedAttributes().ToList();
            var n = attrs1.Count;

            Assert.AreEqual(n, attrs2.Count);
            for (var i = 0; i < n; ++i)
            {
                var attr1 = attrs1[i];
                var attr2 = attrs2[i];
                Assert.AreEqual(attr1.Descriptor.ToString(), attr2.Descriptor.ToString());
                Assert.AreEqual(attr1.Bytes.ByteCount, attr2.Bytes.ByteCount);
                Assert.AreEqual(attr1.Count, attr2.Count);

                // TODO: perhaps in some cases we want an exact byte compare (which should be triggered using a flag).
            }
        }

        public static void OutputTriangleStats(Triangle t)
        {
            Console.WriteLine($"Vertices: {t.A} {t.B} {t.C}");
            Console.WriteLine($"Area: {t.Area} Perimeter: {t.Perimeter} Midpoint: {t.MidPoint}");
            Console.WriteLine($"Bounding box: {t.BoundingBox}");
            Console.WriteLine($"Bounding sphere: {t.BoundingSphere}");
            Console.WriteLine($"Normal: {t.Normal}, normal direction {t.NormalDirection}");
            Console.WriteLine($"Lengths: {t.LengthA} {t.LengthB} {t.LengthC}");
        }

        public static void OutputTriangleStatsSummary(IGeometry g)
        {
            var triangles = g.Triangles();
            for (var i = 0; i < Math.Min(3, triangles.Count); ++i)
            {
                Console.WriteLine($"Triangle {i}");
                OutputTriangleStats(triangles[i]);
            }

            if (triangles.Count > 3)
            {
                Console.WriteLine("...");
                Console.WriteLine($"Triangle {triangles.Count - 1}");
                OutputTriangleStats(triangles.Last());
            }
        }

        public static void OutputIGeometryStats(IGeometry g)
        {
            var stats = g.GetStats();
            Console.WriteLine(stats);
            Assert.IsTrue(g.AreAllIndicesValid());
            foreach (var attr in g.SortedAttributes())
                Console.WriteLine($"{attr.Descriptor} #bytes={attr.Bytes.ByteCount} count={attr.Count}");
        }

        public static void GeometryNullOps(IGeometry g)
        {
            CompareGeometries(g, g);
            CompareGeometries(g, g.ToG3D().ToIGeometry());
            CompareGeometries(g, g.SortedAttributes().ToIGeometry());
            CompareGeometries(g, g.ReplaceAttribute(g.VertexAttribute).ToIGeometry());
            CompareGeometries(g, g.Translate(Vector3.Zero));
            CompareGeometries(g, g.Scale(1.0f));
            CompareGeometries(g, g.Scale(10f).Scale(0.1f));
            CompareGeometries(g, g.Transform(Matrix4x4.Identity));

            // Converting to G3Sharp is not going to give the same result if we convert.
            BasicCompareGeometries(g.ToTriMesh(), g.ToG3Sharp().ToIGeometry());

            //CompareGeometries(g, g.WeldVertices());
            //CompareGeometries(g, g.RemoveUnusedVertices());

            //CompareGeometries(g, g.CopyFaces(0, g.NumFaces));
            //CompareGeometries(g, g.Merge(g).CopyFaces(0, g.NumFaces));
        }

        [Test]
        public static void BasicTests()
        {
            foreach (var g in AllGeometries)
            {
                ValidateGeometry(g);
                ValidateGeometry(g.ToTriMesh());
            }

            Assert.AreEqual(3, XYTriangle.PointsPerFace);
            Assert.AreEqual(1, XYTriangle.FaceCount());
            Assert.AreEqual(3, XYTriangle.Vertices.Count);
            Assert.AreEqual(3, XYTriangle.Indices.Count);
            Assert.AreEqual(1, XYTriangle.Triangles().Count);
            Assert.AreEqual(0.5, XYTriangle.Area(), SmallTolerance);
            Assert.IsTrue(XYTriangle.Planar());
            Assert.AreEqual(new[] { 3 }, XYTriangle.FaceSizes.ToArray());
            Assert.AreEqual(new[] { 0 }, XYTriangle.Topology.FacesToCorners.ToArray());
            Assert.AreEqual(new[] { 0, 1, 2 }, XYTriangle.Indices.ToArray());

            Assert.AreEqual(4, XYQuad.PointsPerFace);
            Assert.AreEqual(1, XYQuad.FaceCount());
            Assert.AreEqual(4, XYQuad.Vertices.Count);
            Assert.AreEqual(4, XYQuad.Indices.Count);
            Assert.AreEqual(2, XYQuad.Triangles().Count);
            Assert.AreEqual(1, XYQuad.Area(), SmallTolerance);
            Assert.IsTrue(XYQuad.Planar());
            Assert.AreEqual(new[] { 4 }, XYQuad.FaceSizes.ToArray());
            Assert.AreEqual(new[] { 0 }, XYQuad.Topology.FacesToCorners.ToArray());
            Assert.AreEqual(new[] { 0, 1, 2, 3 }, XYQuad.Indices.ToArray());

            Assert.AreEqual(4, XYQuadFromFunc.PointsPerFace);
            Assert.AreEqual(1, XYQuadFromFunc.FaceCount());
            Assert.AreEqual(4, XYQuadFromFunc.Vertices.Count);
            Assert.AreEqual(4, XYQuadFromFunc.Indices.Count);
            Assert.AreEqual(2, XYQuadFromFunc.Triangles().Count);
            Assert.AreEqual(1, XYQuadFromFunc.Area(), SmallTolerance);
            Assert.IsTrue(XYQuadFromFunc.Planar());
            Assert.AreEqual(new[] { 4 }, XYQuadFromFunc.FaceSizes.ToArray());
            Assert.AreEqual(new[] { 0 }, XYQuadFromFunc.Topology.FacesToCorners.ToArray());
            //Assert.AreEqual(new[] { 0, 1, 2, 3 }, XYQuadFromFunc.Indices.ToArray());

            Assert.AreEqual(4, XYQuad2x2.PointsPerFace);
            Assert.AreEqual(4, XYQuad2x2.FaceCount());
            Assert.AreEqual(9, XYQuad2x2.Vertices.Count);
            Assert.AreEqual(16, XYQuad2x2.Indices.Count);
            Assert.AreEqual(8, XYQuad2x2.Triangles().Count);
            Assert.AreEqual(1, XYQuad2x2.Area(), SmallTolerance);
            Assert.IsTrue(XYQuad2x2.Planar());
            Assert.AreEqual(new[] { 4, 4, 4, 4 }, XYQuad2x2.FaceSizes.ToArray());
            Assert.AreEqual(new[] { 0, 4, 8, 12 }, XYQuad2x2.Topology.FacesToCorners.ToArray());
            //Assert.AreEqual(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, XYQuad2x2.Indices.ToArray());

            Assert.AreEqual(3, Tetrahedron.PointsPerFace);
            Assert.AreEqual(4, Tetrahedron.FaceCount());
            Assert.AreEqual(4, Tetrahedron.Vertices.Count);
            Assert.AreEqual(12, Tetrahedron.Indices.Count);
            Assert.AreEqual(new[] { 3, 3, 3, 3 }, Tetrahedron.FaceSizes.ToArray());
            Assert.AreEqual(new[] { 0, 3, 6, 9 }, Tetrahedron.Topology.FacesToCorners.ToArray());
            Assert.AreEqual(TestTetrahedronIndices, Tetrahedron.Indices.ToArray());

            Assert.AreEqual(3, XYTriangleTwice.PointsPerFace);
            Assert.AreEqual(2, XYTriangleTwice.FaceCount());
            Assert.AreEqual(6, XYTriangleTwice.Vertices.Count);
            Assert.AreEqual(6, XYTriangleTwice.Indices.Count);
            Assert.AreEqual(2, XYTriangleTwice.Triangles().Count);
            Assert.AreEqual(1.0, XYTriangleTwice.Area(), SmallTolerance);
            Assert.IsTrue(XYTriangleTwice.Planar());
            Assert.AreEqual(new[] { 3, 3 }, XYTriangleTwice.FaceSizes.ToArray());
            Assert.AreEqual(new[] { 0, 3, }, XYTriangleTwice.Topology.FacesToCorners.ToArray());
            Assert.AreEqual(new[] { 0, 1, 2, 3, 4, 5 }, XYTriangleTwice.Indices.ToArray());
        }

        [Test]
        public static void BasicManipulationTests()
        {
            foreach (var g in AllGeometries)
                GeometryNullOps(g);
        }

        public static void ValidateGeometry(IGeometry g)
        {
            g.Validate();
            Assert.IsTrue(g.AreAllIndicesValid());
            Assert.IsTrue(!g.AreTrianglesRepeated());
            Assert.IsTrue(!g.HasDegenerateFaceIndices());
        }

        [Test]
        public static void OutputGeometryData()
        {
            var n = 0;
            foreach (var g in AllGeometries)
            {
                Console.WriteLine($"Geometry {n++}");
                for (var i = 0; i < g.Vertices.Count && i < 10; ++i)
                {
                    Console.WriteLine($"Vertex {i} {g.Vertices[i]}");
                }

                if (g.Vertices.Count > 10)
                {
                    var last = g.Vertices.Count - 1;
                    Console.WriteLine("...");
                    Console.WriteLine($"Vertex {last} {g.Vertices[last]}");
                }

                for (var i = 0; i < g.NumFaces && i < 10; ++i)
                {
                    Console.WriteLine($"Face {i}: {g.GetFace(i)}");
                }

                if (g.Vertices.Count > 10)
                {
                    var last = g.NumFaces - 1;
                    Console.WriteLine("...");
                    Console.WriteLine($"Face {last}: {g.GetFace(last)}");
                }
            }
        }
    }
}

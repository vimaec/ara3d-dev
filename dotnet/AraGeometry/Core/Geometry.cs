using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

/* 
 * The question are:
 * 1. am I going to use Vector or IArray. 
 * 2. what is the basic interface of IGeometry? 
 * 3. is it parameterized over faces? 
 * 
 * I am going to have LineSet, TriMesh, QuadMesh, and PolyMesh. I need my operations
 * to work on all of these. 
 * 
 * Vertices, Elements 
 * 
 * An element is a struct ... that points to the IGeometry 
 */
namespace Ara3D
{
    public interface IElement : IArray<int>
    {
        IGeometry Geometry { get; }
    }

    // Notice: this is the same as the G3D attribute association order 
    public enum AttributeAssociation
    {
        Unknown = 0,
        Point = 1,
        Polygon = 2, 
        PolygonVertex = 3,
        Edge = 4,
        Object = 5,
    }

    public interface IAttributeDescriptor
    {
        string Name { get; }
        AttributeAssociation Mapping { get; }
        Type DataType { get; }
    }

    public interface IAttributeChannel
    {
        IAttributeDescriptor Descriptor { get; }
    }

    public interface IAttributeChannel<T> : IAttributeChannel
    {
        IArray<T> DataBuffer { get; }
    }

    public interface IGeometry 
    {
        int Arity { get; }
        IArray<Vector3> Vertices { get; }
        IArray<int> Indices { get; }
        IArray<IElement> Elements { get; }
        //IArray<IAttributeChannel> Attributes { get; }
    }

    public struct Geometry : IGeometry
    {
        public Geometry(int arity, IArray<Vector3> vertices, IArray<int> indices, IArray<IElement> elements)
        {
            Arity = arity;
            Vertices = vertices;
            Indices = indices;
            Elements = elements;
        }
        public int Arity { get; }
        public IArray<Vector3> Vertices { get; }
        public IArray<int> Indices { get; }
        public IArray<IElement> Elements { get; }
    }

    public struct PolyFace : IElement
    {
        public IGeometry Geometry { get; }
        public PolyFace(IGeometry g, IArray<int> indices)
        {
            Geometry = g;
            Indices = indices;
        }
        public int Count => Indices.Count;
        public IArray<int> Indices { get; }
        public int this[int n] { get { return Indices[n]; } }
    }

    public struct QuadFace : IElement
    {
        public IGeometry Geometry { get; }
        public QuadFace(IGeometry g, int a, int b, int c, int d)
        {
            Geometry = g;
            A = a; B = b; C = c; D = d;
        }
        public int A, B, C, D;
        public int Count => 4;
        public int this[int n] { get { return n == 0 ? A : n == 1 ? B : n == 2 ? C : D; } }
    }

    public struct TriFace : IElement
    {
        public TriFace(IGeometry g, int a, int b, int c)
        {
            Geometry = g;
            A = a; B = b; C = c;
        }
        public int A, B, C;
        public IGeometry Geometry { get; }
        public int Count => 3;
        public int this[int n] { get { return n == 0 ? A : n == 1 ? B : C; } }
    }

    public struct LineSegment : IElement
    {
        public IGeometry Geometry { get; }
        public int A, B;
        public int Count => 2;
        public int this[int n] { get { return n == 0 ? A : B; } }
    }

    public struct Point : IElement
    {
        public IGeometry Geometry { get; }
        public int Count => 1;
        public int Index { get; }
        public int this[int n] { get { return Index; } }
    }

    public struct SphereElement : IElement
    {
        public IGeometry Geometry { get; }
        public int Count => 1;
        public int Index { get; }
        public int this[int n] { get { return Index; } }
    }

    public struct BoxElement : IElement
    {
        public IGeometry Geometry { get; }
        public int Count => 1;
        public int Index { get; }
        public int this[int n] { get { return Index; } }
    }

    public struct Edge : IArray<Vector3>
    {
        public Edge(IElement e, int i) { Element = e; Index = i; }
        public IElement Element { get; }
        public int Index { get; }       
        public int Count { get { return 2; } }
        public Vector3 this[int n] { get { return Element.Points().ElementAtModulo(Index + n); } }
    }

    public struct Corner
    {
        public IElement Element { get; }
        public int Index { get; }
    }

    public struct Vertex
    {
        public IGeometry Geometry { get; }
        public int Index { get; }
    }

    /*
    public class BaseMesh: IGeometry
    {
        public BaseMesh(IArray<Vector3> vertices, IArray<int> indices, IArray<int> faceIndices, IArray<int> faceCounts, IArray<IElement> elements)
        {
            Vertices = vertices;
            Indices = indices;
            FaceIndices = faceIndices;
            FaceCounts = faceCounts;
            Elements = elements;
        }
        public IArray<int> Indices { get; }
        public IArray<int> FaceIndices { get; }
        public IArray<int> FaceCounts { get; }
        public IArray<Vector3> Vertices { get; }
        public IArray<IElement> Elements { get; }
    }
    */

    public struct Polygon : IGeometry, IElement
    {
        public Polygon(IArray<Vector3> vertices)
        {
            Vertices = vertices;
        }

        public int Arity => 0;
        public int this[int n] { get { return n; }  }
        public IArray<Vector3> Vertices { get; }
        public IArray<int> Indices { get { return Vertices.Indices(); } }
        public IArray<IElement> Elements { get { return (this as IElement).Repeat(1); } }
        public IGeometry Geometry { get { return this; }  }
        public int Count { get { return Indices.Count; }  }
    }

    public class TriMesh : IGeometry
    {
        public TriMesh()
            : this(Vector3.Zero.Repeat(0))
        { }

        public TriMesh(IArray<Vector3> vertices) 
            : this(vertices, vertices.Indices())
        {  }

        public TriMesh(IArray<Vector3> vertices, IArray<int> indices)
        {
            Vertices = vertices;
            Indices = indices;
            Faces = (Indices.Count / 3).Select(i => new TriFace(this, Indices[i * 3], Indices[i * 3 + 1], Indices[i * 3 + 2]));
        }

        public int Arity => 3;
        public IArray<int> Indices { get; }
        public IArray<TriFace> Faces { get; }
        public IArray<IElement> Elements => Faces.Select(f => f as IElement);
        public IArray<Vector3> Vertices { get; }

        public static TriMesh Empty = new TriMesh();
    }

    public class QuadMesh : IGeometry
    {
        public QuadMesh()
            : this(Vector3.Zero.Repeat(0))
        { }

        public QuadMesh(IArray<Vector3> vertices)
            : this(vertices, vertices.Indices())
        { }

        public QuadMesh(IArray<Vector3> vertices, IArray<int> indices)
        {
            Vertices = vertices;
            Indices = indices;
            Elements = (Indices.Count / 4).Select(i => new QuadFace(this, Indices[i * 4], Indices[i * 4 + 1], Indices[i * 4 + 2], Indices[i * 4 + 3]) as IElement);
        }

        public int Arity => 4;
        public IArray<int> Indices { get; }
        public IArray<IElement> Elements { get; }
        public IArray<Vector3> Vertices { get; }

        public static TriMesh Empty = new TriMesh();
    }

    public class PolyMesh : IGeometry
    {
        // TODO: this should be constructed from an index buffer and an array of face counts 
        public PolyMesh(IArray<Vector3> vertices, IArray<IArray<int>> faces)
        {
            Elements = faces.Select(f => new PolyFace(this, f) as IElement);
            Indices = faces.Flatten();
            Vertices = vertices;
        }

        public int Arity => 0;
        public IArray<int> Indices { get; }
        public IArray<IElement> Elements { get; }
        public IArray<Vector3> Vertices { get; }
    }

    /*
    public class PolyMesh : Geometry<PolyFace> { }
    public class TriMesh : Geometry<TriFace> { }
    public class QuadMesh : Geometry<QuadFace> { }
    public class Lines : Geometry<Line> { }
    public class Points : Geometry<Line> { }
    public class Spheres : Geometry<SphereElement> { }
    public class Boxes : Geometry<BoxElement> { }
    */

    public class PolyMeshBuilder
    {
        readonly List<IArray<Vector3>> polygons = new List<IArray<Vector3>>();

        public PolyMeshBuilder Add(IArray<Vector3> polygon)
        {
            polygons.Add(polygon);
            return this;
        }

        public PolyMesh ToPolyMesh()
        {
            var polys = polygons;
            var vertices = polys.ToIArray().Flatten();
            var faceCounts = polys.Select(p => p.Count);
            var faceIndices = faceCounts.ToIArray().PostAccumulate((x, y) => x + y);
            var indices = polys.Select((p, i) => p.Indices().Add(faceIndices[i]));
            return new PolyMesh(vertices, indices.ToIArray());
        }
    }

    public class TriMeshBuilder
    {
        List<Vector3> vertexBuffer = new List<Vector3>();
        List<int> indexBuffer = new List<int>();

        public TriMeshBuilder Add(Vector3 v)
        {
            vertexBuffer.Add(v);
            return this;
        }
        public TriMeshBuilder AddFace(int a, int b, int c)
        {
            indexBuffer.Add(a);
            indexBuffer.Add(b);
            indexBuffer.Add(c);
            return this;
        }

        public TriMesh ToMesh()
        {            
            var obj = new object();
            lock (obj) {
                var r = new TriMesh(vertexBuffer.ToIArray(), indexBuffer.ToIArray());
                vertexBuffer = null;
                indexBuffer = null;
                return r;
            }
        }
    }

    public static class GeometryExtensions
    {
        // Epsilon is bigger than the real epsilon. 
        public const float EPSILON = float.Epsilon * 100;

        /* TODO: urgent fix this. 
        public static IArray<IArray<Edge>> Edges(this IGeometry self)
        {
            return self.Elements.Select(Edges);
        }
        */

        /*
        public static IArray<Edge> Edges(this IElement self)
        {
            return self.Select(i => new Edge(self, i));
        }*/

        public static Vector3 MidPoint(this IElement self)
        {
            return self.Points().Average();
        }

        public static Vector3 MidPoint(Vector3 a, Vector3 b)
        {
            return (a + b) * 0.5f;
        }

        public static Vector3 MidPoint(this Edge self)
        {
            return MidPoint(self[0], self[1]);
        }

        /*
        public static IArray<Vector3> EdgeMidPoints(this IElement self)
        {
            return self.Edges().Select(MidPoint);
        }
        */

        // TODO: this could be broken.
        public static IArray<Vector3> Points(this IElement self)
        {
            // TODO: urgent, fix this
            return self.Geometry.Vertices.SelectByIndex(self);
        }

        public static TriFace TriFace(this IGeometry self, int a, int b, int c)
        {
            return new TriFace(self, a, b, c);
        }

        public static QuadFace QuadFace(this IGeometry self, int a, int b, int c, int d)
        {
            return new QuadFace(self, a, b, c, d);
        }

        public static PolyFace PolyFace(this IGeometry self, IArray<int> indices)
        {
            return new PolyFace(self, indices);
        }

        public static int FaceCount(this IGeometry self)
        {
            return self.Elements.Count;
        }

        /*
        public static IGeometry ToPolyMesh(this IGeometry self, IEnumerable<IEnumerable<int>> indices)
        {
            var verts = self.Vertices;
            var flatIndices = indices.SelectMany(xs => xs).ToIArray();
            var faceIndices = indices.Where(xs => xs.Any()).Select(xs => xs.First()).ToIArray();
            return new BaseMesh(verts, flatIndices, faceIndices);
        }
        */

        /*
      public static IGeometry MergeCoplanar(this IGeometry self)
      {
          if (self.Elements.Count <= 1) return self;
          var curPoly = new List<int>();
          var polys = new List<List<int>> { curPoly };
          var cur = 0;
          for (var i=1; i < self.Elements.Count; ++i)
          {
              if (!self.CanMergeTris(cur, i))
              {
                  cur = i;
                  polys.Add(curPoly = new List<int>());
              }
              curPoly.Add(self.Elements[i].ToList());
          }
          return self.ToPolyMesh(polys);
      }     
      */

        public static Vector3 Tangent(this IElement self)
        {
            return self.Points()[1] - self.Points()[0];
        }

        public static Vector3 Binormal(this IElement self)
        {
            return self.Points()[2] - self.Points()[0];
        }

        public static IArray<Triangle> Triangles(this IElement self)
        {
            if (self.Count < 3) return Triangle.Zero.Repeat(0);
            var pts = self.Points();
            if (self.Count == 3) return new Triangle(pts[0], pts[1], pts[2]).Repeat(1);
            return (self.Count - 2).Select(i => new Triangle(pts[0], pts[i], pts[i + 1]));
        }

        public static bool Coplanar(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float epsilon = EPSILON)
        {
            // https://en.wikipedia.org/wiki/Coplanarity
            return Math.Abs(Vector3.Dot(v3 - v1, Vector3.Cross(v2 - v1, v4 - v1))) < epsilon;
        }

        public static Vector3 Normal(this IElement self)
        {
            return Vector3.Normalize(Vector3.Cross(self.Binormal(), self.Tangent()));
        }

        // public static IGeometry ToQuadMesh(IArray<IArray<Vector3>> rows, bool connectRows, bool connectCols) { }

        public static IGeometry ToQuadMesh(Func<Vector2, Vector3> f, int usegs, int vsegs)
        {
            var verts = new List<Vector3>();
            var indices = new List<int>();
            for (var i = 0; i <= usegs; ++i)
            {
                var u = (float)i / usegs;
                for (var j = 0; j <= vsegs; ++j)
                {
                    var v = (float)j / vsegs;
                    verts.Add(f(new Vector2(u, v)));

                    if (i < usegs && j < vsegs)
                    {
                        indices.Add(i * (vsegs + 1) + j);
                        indices.Add(i * (vsegs + 1) + j + 1);
                        indices.Add((i + 1) * (vsegs + 1) + j + 1);
                        indices.Add((i + 1) * (vsegs + 1) + j);
                    }
                }
            }
            return new QuadMesh(verts.ToIArray(), indices.ToIArray());
        }

        public static bool CanMergeTris(this IGeometry self, int a, int b)
        {
            var e1 = self.Elements[a];
            var e2 = self.Elements[b];
            if (e1.Count != e2.Count && e1.Count != 3) return false;
            var indices = new[] { e1[0], e1[1], e1[2], e2[0], e2[1], e2[2] }.Distinct().ToList();
            if (indices.Count != 4) return false;
            var verts = self.Vertices.SelectByIndex(indices.ToIArray());
            return Coplanar(verts[0], verts[1], verts[2], verts[3]);
        }
        public static IArray<Vector3> UsedVertices(this IGeometry self)
        {
            return self.Elements.SelectMany(es => es.Points());
        }
        public static IArray<Vector3> FaceMidPoints(this IGeometry self)
        {
            return self.Elements.Select(e => e.MidPoint());
        }

        public static IGeometry WeldVertices(this IGeometry self)
        {
            var verts = new Dictionary<Vector3, int>();
            var indices = new List<int>();
            for (var i = 0; i < self.Vertices.Count; ++i)
            {
                var v = self.Vertices[i];
                if (verts.ContainsKey(v))
                {
                    indices.Add(verts[v]);
                }
                else
                {
                    var n = verts.Count;
                    indices.Add(n);
                    verts.Add(v, n);
                }
            }
            return new Geometry(self.Arity, verts.Keys.ToIArray(), indices.ToIArray(), self.Elements);
        }

        public static IGeometry Deform(this IGeometry self, Func<Vector3, Vector3> f)
        {
            return new Geometry(self.Arity, self.Vertices.Select(f), self.Indices, self.Elements);
        }

        public static IGeometry Transform(this IGeometry self, Matrix4x4 m)
        {
            return self.Deform(v => v.Transform(m));
        }

        public static IGeometry Translate(this IGeometry self, Vector3 offset)
        {
            return self.Deform(v => v + offset);
        }

        public static Box BoundingBox(this IArray<Vector3> vertices)
        {
            return Box.Create(vertices.ToEnumerable());
        }

        public static Box BoundingBox(this IGeometry self)
        {
            return self.Vertices.BoundingBox();
        }

        public static string GetStats(this IGeometry self)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Number of vertices {self.Vertices.Count}");
            sb.AppendLine($"Number of indices {self.Indices.Count}");
            sb.AppendLine($"Number of elements {self.Elements.Count}");
            sb.AppendLine($"Bounding box {self.BoundingBox()}");
            // TODO: distance from ground plane (box extent)
            // TODO: closest distance to origin (from box extent)
            // TODO: standard deviation 
            // TODO: scene analysis as well 
            // TODO: number of distinct vertices 
            // TODO: volume of bounding box
            // TODO: surface area of bounding box on ground plane
            var tris = self.Triangles();
            sb.AppendLine($"Triangles {tris.Count}");
            sb.AppendLine($"Distinct triangles {tris.ToEnumerable().Distinct().Count()}");
            var smallArea = 0.00001;
            sb.AppendLine($"Triangles with small area {tris.CountWhere(tri => tri.Area < smallArea)}");
            return sb.ToString();
        }

        public static IArray<Triangle> Triangles(this IGeometry self)
        {
            if (self is TriMesh tm)
            {
                return self.Elements.Select(e => e.Triangles()[0]);
            }
            return self.Elements.SelectMany(e => e.Triangles());
        }

        // This assumes that every polygon is convex, and without holes. Line or point elements are not converted into triangles. 
        // TODO: move all data channels along for the ride. 
        public static TriMesh ToTriMesh(this IGeometry self)
        {
            if (self is TriMesh tris)
                return tris;
            var indices = new List<int>();
            for (var i = 0; i < self.Elements.Count; ++i)
            {
                var e = self.Elements[i];
                for (var j = 1; j < e.Count - 1; ++j)
                {
                    indices.Add(e[0]);
                    indices.Add(e[j]);
                    indices.Add(e[j + 1]);
                }
            }
            return new TriMesh(self.Vertices, indices.ToIArray());
        }

        public static IGeometry Merge(this IArray<IGeometry> geometries)
        {
            var verts = new Vector3[geometries.Sum(g => g.Vertices.Count)];
            var faces = new List<IArray<int>>();
            var offset = 0;
            foreach (var g in geometries.ToEnumerable())
            {
                g.Vertices.CopyTo(verts, offset);
                g.Elements.Select(e => e.Add(offset)).AddTo(faces);
                offset += g.Vertices.Count;
            }
            return new PolyMesh(verts.ToIArray(), faces.ToIArray());
        }

        public static bool AreAllIndicesValid(this IGeometry self)
        {
            return self.Indices.All(i => i.Between(0, self.Vertices.Count - 1));
        }

        public static bool AreAllVerticesUsed(this IGeometry self)
        {
            var bools = new bool[self.Vertices.Count];
            foreach (var i in self.Indices.ToEnumerable())
                bools[i] = true;
            return bools.All(b => b);
        }

        public static bool IsValid(this IGeometry self)
        {
            return self.AreAllIndicesValid();
        }
    }
}

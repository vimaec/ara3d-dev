using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Ara3D
{
    public interface IGeometry : IG3D
    {
        int PointsPerFace { get; }
        int NumFaces { get; }
        IArray<Vector3> Vertices { get; } 
        IArray<int> Indices { get; }  
        IArray<int> FaceSizes { get; }
        IArray<Vector2> UVs { get; }
        Topology Topology { get; }
    }

    /// <summary>
    /// This class is used to make efficient topological queries for an IGeometry.
    /// Construction is an O(N) operation, so it is not always created automatically. 
    /// </summary>
    public class Topology
    {
        public Topology(IGeometry g)
        {
            Geometry = g;
            IndexBufferToFaces = new int[g.Indices.Count];
            FaceIndices = new int[g.NumFaces];
            var cur = 0;
            for (var i = 0; i < g.NumFaces; ++i)
            {
                FaceIndices[i] = cur;
                var faceSize = g.FaceSizes[i];
                for (var j = 0; j < faceSize; ++j)
                    IndexBufferToFaces[cur++] = i;
            }

            VertexBufferToIndexBuffer = new List<int>[g.Vertices.Count];
            for (var i = 0; i < g.Indices.Count; ++i)
            {
                var index = g.Indices[i];
                if (VertexBufferToIndexBuffer[index] == null)
                    VertexBufferToIndexBuffer[index] = new List<int> {i};
                else
                    VertexBufferToIndexBuffer[index].Add(i);
            }
        }

        public IGeometry Geometry { get; }
        public List<int>[] VertexBufferToIndexBuffer { get; } 
        public int[] IndexBufferToFaces { get; }
        public int[] FaceIndices { get; }

        public int FaceFromIndexBufferIndex(int i)
            => FaceIndices[i];

        public IEnumerable<int> FacesFromVertexIndex(int v)
            => VertexBufferToIndexBuffer[v]?.Select(FaceFromIndexBufferIndex).Distinct() ?? Enumerable.Empty<int>();

        public IArray<int> IndexIndicesFromFace(int f)
            => Geometry.FaceSizes[f].Range().Add(FaceIndices[f]);

        public IArray<int> VertexIndicesFromFace(int f)
            => Geometry.Indices.SelectByIndex(IndexIndicesFromFace(f));

        public IEnumerable<int> NeighbourFaces(int f)
            => VertexIndicesFromFace(f).ToEnumerable().SelectMany(FacesFromVertexIndex).Where(f2 => f2 != f).Distinct();
    }

    public class GeometryDebugView
    {
        IGeometry Interface { get; }

        public int PointsPerFace => Interface.PointsPerFace;
        public int NumFaces => Interface.NumFaces;
        public Vector3[] Vertices => Interface.Vertices.ToArray();
        public int[] Indices => Interface.Indices.ToArray();
        public int[] FaceSizes => Interface.FaceSizes.ToArray();

        public GeometryDebugView(IGeometry g)
        {
            Interface = g;
        }
    }

    /*
    // https://www.scratchapixel.com/lessons/advanced-rendering/introduction-acceleration-structure/introduction
    // https://stackoverflow.com/questions/99796/when-to-use-binary-space-partitioning-quadtree-octree
    // http://gamma.cs.unc.edu/RS/paper_rt07.pdf
    public interface IGeometryAccelerations
    {
        Box Box { get; }
        object BVH { get; }
        object Octree { get; }
        IArray<int> VertexIndexLookup { get; }
        object BSP { get; }
        object AABBTree { get; }
        object RayStrips { get; }
    }

    public interface ICommonAttributeData
    {
        IArray<Vector2> Uvs(int n);
        IArray<Vector3> Uvws(int n);
        IArray<Vector3> Vertices { get; }
        IArray<int> Indices { get; }
        IArray<int> FaceSizes { get; }
        IArray<int> FaceIndices { get; }
        IArray<Vector3> MapChannelData(int n);
        IArray<int> MapChannelIndices(int n);
        IArray<Vector3> FaceNormals { get; }
        IArray<Vector3> VertexNormals { get; }
        IArray<Vector3> VertexBinormals { get; }
        IArray<Vector3> VertexTangents { get; }
        IArray<int> MaterialIds { get; }
        IArray<int> PolyGroups { get; }
        IArray<float> PerVertex(int n);
        IArray<Vector3> VertexColors { get; }
        IArray<int> SmoothingGroups { get; }
        IArray<byte> EdgeVisibility { get; }
        IArray<float> FaceSelection { get; }
        IArray<float> EdgeSelection { get; }
        IArray<float> VertexSelection { get; }
    }
    */

    /// <summary>
    /// A face is an array of indices into the vertex buffer of an IGeometry representing a particular
    /// element of a geometry (could be a PolyMesh face, a TriMesh faces, a QuadMesh face, or even a line segment,
    /// or point if the IGeometry represetns a point cloud. 
    /// </summary>
    public struct Face : IArray<int>, IEquatable<Face>
    {
        public IGeometry Geometry { get; }
        public int Index { get; }
        public int Count => Geometry.FaceSizes[Index];
        public int this[int n] => Geometry.Indices[Geometry.Topology.FaceIndices[Index] + n];

        public bool HasDegenerateIndices()
        {
            for (var i=0; i < Count - 1; ++i)
                for (var j=i+1; j < Count; ++j)
                    if (this[i] == this[j])
                        return true;
            return false;
        }

        public Face(IGeometry g, int index)
        {
            Geometry = g;
            Index = index;
        }

        public override string ToString()
            => string.Join(" ", this.ToEnumerable());

        public bool Equals(Face other)
            => this.Sort().SequenceEquals(other.Sort());

        public override bool Equals(object obj)
            => obj is Face f && Equals(f);

        public override int GetHashCode()
            => Hash.Combine(this.Sort().ToArray());

        public IEnumerable<Face> NeighbourFaces()
        {
            var g = Geometry;
            return Geometry.Topology.NeighbourFaces(Index).Select(i => new Face(g, i));
        }
    }

    public static class Geometry
    {
        // Epsilon is bigger than the real epsilon. 
        public const float EPSILON = float.Epsilon * 100;

        public readonly static IGeometry EmptyTriMesh
            = TriMesh(LinqArray.Empty<Vector3>(), LinqArray.Empty<int>());

        public readonly static IGeometry EmptyQuadMesh
            = QuadMesh(LinqArray.Empty<Vector3>(), LinqArray.Empty<int>());

        /*
        public readonly static IGeometry Box
            = QuadMesh(new Box(Vector3.Zero, Vector3.One).Corners, 
            */

        public static Vector3 MidPoint(this Face self)
            => self.Points().Average();        

        /*
        public static Vector3 MidPoint(this Edge self)
        {
            return MidPoint(self[0], self[1]);
        }
        */

        /*
        public static IArray<Vector3> EdgeMidPoints(this Face self)
        {
            return self.Edges().Select(MidPoint);
        }
        */

        public static IArray<Vector3> Points(this Face self)
            => self.Geometry.Vertices.SelectByIndex(self);        

        public static int FaceCount(this IGeometry self)
            => self.GetFaces().Count;        

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
          if (self.Elements.ByteCount <= 1) return self;
          var curPoly = new List<int>();
          var polys = new List<List<int>> { curPoly };
          var cur = 0;
          for (var i=1; i < self.Elements.ByteCount; ++i)
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

        public static Vector3 Tangent(this Face self)
            => self.Points()[1] - self.Points()[0];
        
        public static Vector3 Binormal(this Face self)
            => self.Points()[2] - self.Points()[0];        

        public static IArray<Triangle> Triangles(this Face self)
        {
            if (self.Count < 3) return new Triangle(Vector3.Zero, Vector3.Zero, Vector3.Zero).Repeat(0);
            var pts = self.Points();
            if (self.Count == 3) return new Triangle(pts[0], pts[1], pts[2]).Repeat(1);
            return (self.Count - 2).Select(i => new Triangle(pts[0], pts[i + 1], pts[i + 2]));
        }

        // https://en.wikipedia.org/wiki/Coplanarity
        public static bool Coplanar(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float epsilon = EPSILON)
            => Math.Abs(Vector3.Dot(v3 - v1, (v2 - v1).Cross(v4 - v1))) < epsilon;        

        public static Vector3 Normal(this Face self)
            => self.Binormal().Cross(self.Tangent()).Normalize();
        
        public static IGeometry Mesh(int sidesPerFace, IArray<Vector3> vertices, IArray<int> indices = null, IArray<Vector2> uvs = null, IArray<int> materialIds = null, IArray<int> objectIds = null)
            => G3DExtensions.ToG3D(sidesPerFace, vertices, indices, uvs, materialIds, objectIds).ToIGeometry();

        public static IGeometry QuadMesh(this IArray<Vector3> vertices, IArray<int> indices = null, IArray<Vector2> uvs = null, IArray<int> materialIds = null, IArray<int> objectIds = null)
            => Mesh(4, vertices, indices, uvs, materialIds, objectIds);

        public static IGeometry TriMesh(this IArray<Vector3> vertices, IArray<int> indices = null, IArray<Vector2> uvs = null, IArray<int> materialIds = null, IArray<int> objectIds = null)
            => Mesh(3, vertices, indices, uvs, materialIds, objectIds);

        /* TODO: finish
        public static IGeometry PolyMesh(this IArray<Vector3> vertices, IArray<Face> faces)
        {
            var vertexAttribute = vertices.ToVertexAttribute();
            var indexBuffer = faces.
        }*/

        /// <summary>
        /// Computes the indices of a quad mesh strip.
        /// TODO: support wrapping around the u or the v, so thatvertex indices are re-used if need be. Otherwise cylinders have coincident vertices 
        /// </summary>
        public static IArray<int> ComputeQuadMeshStripIndices(int usegs, int vsegs)
        {
            var indices = new List<int>();
            for (var i = 0; i < vsegs; ++i)
            {
                for (var j = 0; j < usegs; ++j)
                {
                    indices.Add(i * (usegs + 1) + j);
                    indices.Add(i * (usegs + 1) + j + 1);
                    indices.Add((i + 1) * (usegs + 1) + j + 1);
                    indices.Add((i + 1) * (usegs + 1) + j);
                }
            }

            return indices.ToIArray();
        }

        public static IGeometry QuadMesh(this Func<Vector2, Vector3> f, int usegs, int vsegs)
        {
            var verts = new List<Vector3>();
            for (var i = 0; i <= vsegs; ++i)
            {
                var v = (float) i / vsegs;
                for (var j = 0; j <= usegs; ++j)
                {
                    var u = (float) j / usegs;
                    verts.Add(f(new Vector2(u, v)));
                }
            }

            return QuadMesh(verts.ToIArray(), ComputeQuadMeshStripIndices(usegs, vsegs));
        }

        public static bool CanMergeTris(this IGeometry self, int a, int b)
        {
            var e1 = self.GetFaces()[a];
            var e2 = self.GetFaces()[b];
            if (e1.Count != e2.Count && e1.Count != 3) return false;
            var indices = new[] { e1[0], e1[1], e1[2], e2[0], e2[1], e2[2] }.Distinct().ToList();
            if (indices.Count != 4) return false;
            var verts = self.Vertices.SelectByIndex(indices.ToIArray());
            return Coplanar(verts[0], verts[1], verts[2], verts[3]);
        }

        public static IArray<Vector3> UsedVertices(this IGeometry self) 
            => self.GetFaces().SelectMany(es => es.Points());

        public static IArray<Vector3> FaceMidPoints(this IGeometry self) 
            => self.GetFaces().Select(e => e.MidPoint());

        // TODO: keep all the other data. 
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
            return Mesh(self.PointsPerFace, verts.Keys.ToIArray(), indices.ToIArray());
        }

        public static IGeometry Deform(this IGeometry self, Func<Vector3, Vector3> f)
            => self?.ReplaceAttribute(self.Vertices.Select(f).ToVertexAttribute())?.ToIGeometry();

        public static IGeometry Transform(this IGeometry self, Matrix4x4 m)
            => self.Deform(v => v.Transform(m));
        
        public static IGeometry Translate(this IGeometry self, Vector3 offset)
            => self.Deform(v => v + offset);

        public static IGeometry Scale(this IGeometry self, float amount)
            => self.Deform(v => v * amount);

        public static Box BoundingBox(this IArray<Vector3> vertices)
            => Box.Create(vertices.ToEnumerable());        

        public static Box BoundingBox(this IGeometry self)
            => self.Vertices.BoundingBox();

        public static bool IsPolyMesh(this IGeometry self)
            => !self.HasFixedFaceSize();
        
        public static string GetStats(this IGeometry self)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Points per face {self.PointsPerFace}");
            sb.AppendLine($"Is PolyMesh {self.IsPolyMesh()}");
            sb.AppendLine($"Number of vertices {self.Vertices.Count}");
            sb.AppendLine($"Number of used vertices {self.UsedVertices().Count}");
            sb.AppendLine($"Number of indices {self.Indices.Count}");
            sb.AppendLine($"Number of faces  {self.GetFaces().Count}");
            sb.AppendLine($"Bounding box {self.BoundingBox()}");
            sb.AppendLine($"Averge vertex {self.Vertices.Average()}");
            // TODO: distance from ground plane (box extent)
            // TODO: closest distance to origin (from box extent)
            // TODO: standard deviation 
            // TODO: scene analysis as well 
            // TODO: number of distinct vertices 
            // TODO: volume of bounding box
            // TODO: surface area of bounding box on ground plane
            // TODO: average vertex 
            // TODO: average normal and average UV 
            var tris = self.Triangles();
            sb.AppendLine($"Triangles {tris.Count}");
            // TODO: this did not return actual distinct triangles and it is slow!!!
            //sb.AppendLine($"Distinct triangles {tris.ToEnumerable().Distinct().Count()}");
            var smallArea = 0.00001;
            sb.AppendLine($"Triangles with small area {tris.CountWhere(tri => tri.Area < smallArea)}");
            return sb.ToString();
        }

        public static double Area(this IGeometry g)
            => g.Triangles().Sum(t => t.Area);

        public static IArray<Triangle> Triangles(this IGeometry self)
            => self.GetFaces().SelectMany(e => e.Triangles());       

        // This assumes that every polygon is convex, and without holes. Line or point elements are not converted into triangles. 
        // TODO: move all data channels along for the ride. 
        public static IGeometry ToTriMesh(this IGeometry self)
        {
            if (self.PointsPerFace == 3)
                return self;
            var indices = new List<int>();
            for (var i = 0; i < self.NumFaces; ++i)
            {
                var e = self.GetFace(i);
                for (var j = 1; j < e.Count - 1; ++j)
                {
                    indices.Add(e[0]);
                    indices.Add(e[j]);
                    indices.Add(e[j + 1]);
                }
            }
            return TriMesh(self.Vertices, indices.ToIArray());
        }

        public static IGeometry Merge(this IEnumerable<IGeometry> geometries) 
            => geometries.ToIArray().Merge();

        // TODO: this function need to be generalized to handle all attributes correctly. In fact I think it should proably happen at the IG3D level.
        public static IGeometry Merge(this IArray<IGeometry> geometries)
        {
            var triMeshes = geometries.Where(g => g != null).Select(g => g.ToTriMesh()).ToArray();
            var newVertCount = triMeshes.Sum(g => g.Vertices.Count);
            var newFaceCount = triMeshes.Sum(g => g.NumFaces);
            var verts = new Vector3[newVertCount];
            var uvs = new Vector2[newVertCount];
            var objectIds = new int[newFaceCount];
            var materialIds = new int[newFaceCount];
            var indices = new List<int>();
            var vtxOffset = 0;
            var faceOffset = 0;

            // TODO: this assumes the presence of object ids and material ids
            foreach (var g in triMeshes)
            {
                g.Vertices.CopyTo(verts, vtxOffset);
                g.UVs.CopyTo(uvs, vtxOffset);
                g.Indices.Add(vtxOffset).AddTo(indices);
                g.MaterialIds()?.CopyTo(materialIds, faceOffset);
                g.ObjectIds()?.CopyTo(objectIds, faceOffset);
                vtxOffset += g.Vertices.Count;
                faceOffset += g.NumFaces;
            }

            // TODO: maybe this could all be done using real arrays, probably be faster to serialize, etc.  But that is for later.
            return TriMesh(verts.ToIArray(), indices.ToIArray(), uvs.ToIArray(), materialIds.ToIArray(), objectIds.ToIArray());
        }

        public static bool AreAllIndicesValid(this IGeometry self)
            => self.Indices.All(i => i.Between(0, self.Vertices.Count - 1));
        
        public static bool AreAllVerticesUsed(this IGeometry self)
        {
            var bools = new bool[self.Vertices.Count];
            foreach (var i in self.Indices.ToEnumerable())
                bools[i] = true;
            return bools.All(b => b);
        }

        public static bool IsValid(this IGeometry self)
            => self.AreAllIndicesValid();

        public static IArray<int> FaceIndicesToCornerIndices(this IGeometry g3d, IArray<int> faceIndices)
        {
            if (g3d.PointsPerFace > 0)
                return faceIndices.GroupIndicesToIndices(g3d.PointsPerFace);
            var r = new List<int>();
            var topo = g3d.Topology;
            for (var i = 0; i < faceIndices.Count; ++i)
            {
                var index = faceIndices[i];
                var faceSize = g3d.FaceSizes[index];
                var faceIndex = topo.FaceIndices[index];
                for (var j=0; j < faceSize; ++j)
                    r.Add(g3d.Indices[faceIndex + j]);
            }

            return r.ToIArray();
        }

        public static IGeometry RemapFaces(this IGeometry g, IArray<int> faceRemap)
        {
            var cornerRemap = g.FaceIndicesToCornerIndices(faceRemap);
            return g.VertexAttributes()
                .Concat(g.NoneAttributes())
                .Concat(g.FaceAttributes().Select(attr => attr.Remap(faceRemap)))
                .Concat(g.EdgeAttributes().Select(attr => attr.Remap(cornerRemap)))
                .Concat(g.CornerAttributes().Select(attr => attr.Remap(cornerRemap)))
                .ToG3D()
                .ToIGeometry();
        }

        public static IGeometry CopyFaces(this IGeometry g, Func<int, bool> predicate)
            => g.RemapFaces(g.NumFaces.Select(i => i).IndicesWhere(predicate).ToIArray());

        public static IGeometry CopyFaces(this IGeometry g, int from, int count)
            => g.CopyFaces(i => i >= from && i < from + count);

        public static IArray<IGeometry> CopyFaceGroups(this IGeometry g, int size)
            => g.GetFaces().Count.DivideRoundUp(size).Select(i => CopyFaces(g, i * size, size));

        public static Face GetFace(this IGeometry g, int i)
            => new Face(g, i);

        public static IArray<Face> GetFaces(this IGeometry g) 
            => g.NumFaces.Select(g.GetFace);

        public static IGeometry Merge(this IGeometry g, params IGeometry[] others)
        {
            var gs = others.ToList();
            gs.Insert(0, g);
            return gs.Merge();
        }

        public static IEnumerable<IAttribute> SortedAttributes(this IGeometry g)
            => g.Attributes.OrderBy(attr => attr.Descriptor.ToString());

        public static IGeometry ToIGeometry(this IEnumerable<IAttribute> attributes)
            => attributes.ToG3D().ToIGeometry();

        public static IGeometry ToIGeometry(this IG3D g)
            => new G3DAdapter(g);

        public static IG3D ToG3D(this IGeometry g)
            => g.Attributes.ToG3D();

        public static bool Planar(this IGeometry g, float tolerance = Constants.Tolerance)
        {
            if (g.NumFaces <= 1) return true;
            var normal = g.GetFace(0).Normal();
            return g.GetFaces().All(f => f.Normal().AlmostEquals(normal, tolerance));
        }

        public static bool AreTrianglesRepeated(this IGeometry g)
            => g.GetFaces().CountUnique() != g.NumFaces;

        public static bool HasDegenerateFaceIndices(this IGeometry g)
            => g.GetFaces().Any(f => f.HasDegenerateIndices());

        public static void Validate(this IGeometry g3d)
        {
            (g3d as IG3D).Validate();

            if (g3d.FaceSizes.Count != g3d.NumFaces)
                throw new Exception("Expected the face sizes array to be equal to the number of faces");
            if (!g3d.AreAllIndicesValid())
                throw new Exception("Not all indices are valid");

            var topo = g3d.Topology;
            var faceIndex = 0;
            for (var i = 0; i < g3d.NumFaces; ++i)
            {
                if (faceIndex != topo.FaceIndices[i])
                    throw new Exception("Topology face indices is incorrect");
                var faceSize = g3d.FaceSizes[i];
                
                var face = g3d.GetFace(i);
                if (face.Count != faceSize)
                    throw new Exception("Face does not have correct size");

                for (var j = 0; j < faceSize; ++j)
                {
                    var index = g3d.Indices[faceIndex + j];
                    if (index < 0 || index >= g3d.Vertices.Count)
                        throw new Exception("Vertex index out of range");

                    if (index != face[j])
                        throw new Exception("Face does not have valid index");
                }

                faceIndex += faceSize;
            }
        }

        /// <summary>
        /// Creates a revolved face ... note that the last points are on top of the original 
        /// </summary>
        public static IGeometry RevolveAroundAxis(this IArray<Vector3> points, Vector3 axis, int segments = 4)
        {
            var verts = new List<Vector3>();
            for (var i = 0; i < segments; ++i)
            {
                var angle = Constants.TwoPi / segments;
                points.Rotate(axis, angle).AddTo(verts);
            }

            return QuadMesh(verts.ToIArray(), ComputeQuadMeshStripIndices(segments-1, points.Count-1));
        }

        public static IGeometry RemoveUnusedVertices(this IGeometry g)
        {
            var tmp = false.Repeat(g.Vertices.Count).ToArray();
            for (var i=0; i < g.Indices.Count; ++i)
                tmp[g.Indices[i]] = true;

            var n = 0;
            var remap = new int[g.Vertices.Count];
            var newVertices = new List<Vector3>();
            for (var i = 0; i < remap.Length; ++i)
            {
                if (tmp[i])
                {
                    remap[i] = n++;                    
                    newVertices.Add(g.Vertices[i]);
                }
                else
                    remap[i] = -1;
            }

            // Just make sure that everything makes sense
            for (var i = 0; i < g.Indices.Count; ++i)
            {
                var index = g.Indices[i];
                Debug.Assert(tmp[index] = true);
                var newIndex = remap[index];
                Debug.Assert(newIndex >= 0);
                var vtx = g.Vertices[index];
                var vtx2 = newVertices[newIndex];
                Debug.Assert(vtx == vtx2);
            }

            // Set up the new indices
            var newIndices = new List<int>();
            for (var i = 0; i < g.Indices.Count; ++i)
            {
                newIndices.Add(remap[g.Indices[i]]);
            }

            return Mesh(g.PointsPerFace, newVertices.ToIArray(), newIndices.ToIArray());
        }

        public static bool IsEqual(this IGeometry g1, IGeometry g2)
            => g1.NumFaces == g2.NumFaces
               && g1.PointsPerFace == g2.PointsPerFace
               && g1.Indices.SequenceEquals(g2.Indices)
               && g1.Vertices.SequenceEquals(g2.Vertices)
               && g1.UVs.SequenceEquals(g2.UVs)
               && g1.FaceSizes.SequenceEquals(g2.FaceSizes);

        /// <summary>
        /// Creates a TriMesh from four points. 
        /// </summary>
        public static IGeometry TriMeshFromQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            => TriMesh(new[] {a, b, c, c, d, a}.ToIArray());

        public static IGeometry SetMaterialIds(this IGeometry g, int id)
            => g.SetMaterialIds(id.Repeat(g.NumFaces));

        public static IGeometry SetMaterialIds(this IGeometry g, IArray<int> ids)
            => g.ReplaceAttribute(ids.ToMaterialIdsAttribute()).ToIGeometry();

        public static IGeometry SetObjectIds(this IGeometry g, int id)
            => g.SetObjectIds(id.Repeat(g.NumFaces));

        public static IGeometry SetObjectIds(this IGeometry g, IArray<int> ids)
            => g.ReplaceAttribute(ids.ToObjectIdsAttribute()).ToIGeometry();
    }
}

﻿using g3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ara3D
{
    public static class G3Sharp
    {
        // https://github.com/gradientspace/geometry3Sharp/issues/3
        public static DMesh3 Compact(this DMesh3 mesh)
        {
            return new DMesh3(mesh, true);
        }

        public static DMesh3 Slice(this DMesh3 self, Plane plane)
        {
            var normal = plane.Normal;
            var origin = normal * -plane.D;
            var cutter = new MeshPlaneCut(self, origin.ToVector3D(), normal.ToVector3D());
            var result = cutter.Cut();
            Console.WriteLine($"Cutting result = {result}");
            Console.WriteLine($"Cut loops = {cutter.CutLoops.Count}");
            Console.WriteLine($"Cut spans = {cutter.CutSpans.Count}");
            Console.WriteLine($"Cut faces = {cutter.CutFaceSet?.Count ?? 0}");
            return cutter.Mesh;
        }

        public static DMesh3 Reduce(this DMesh3 mesh, float percent, bool project = true)
        {
            if (!mesh.CheckValidity(eFailMode: FailMode.ReturnOnly))
                return mesh;
            var r = new Reducer(mesh);
            if (project)
            {
                var tree = mesh.AABBTree();
                r.SetProjectionTarget(new MeshProjectionTarget(tree.Mesh, tree));

                // http://www.gradientspace.com/tutorials/2017/8/30/mesh-simplification
                // r.ProjectionMode = Reducer.TargetProjectionMode.Inline;
            }

            var target = mesh.VertexCount * percent / 100.0f;
            r.ReduceToVertexCount((int) target);
            var newMesh = r.Mesh.Compact();
            var g = newMesh.ToIGeometry();
            Debug.Assert(g.AreAllIndicesValid());
            //Debug.Assert(g.AreAllVerticesUsed());
            return newMesh;
        }

        public static DMeshAABBTree3 AABBTree(this DMesh3 mesh)
        {
            var tree = new DMeshAABBTree3(mesh);
            tree.Build();
            return tree;
        }

        public static double? DistanceToTree(this DMeshAABBTree3 tree, Ray3d ray)
        {
            var hit_tid = tree.FindNearestHitTriangle(ray);
            if (hit_tid == DMesh3.InvalidID) return null;
            var intr = MeshQueries.TriangleIntersection(tree.Mesh, hit_tid, ray);
            return ray.Origin.Distance(ray.PointAt(intr.RayParameter));
        }

        public static Vector3d NearestPoint(this DMeshAABBTree3 tree, Vector3d point)
        {
            var tid = tree.FindNearestTriangle(point);
            if (tid == DMesh3.InvalidID)
                return new Vector3d();

            var dist = MeshQueries.TriangleDistance(tree.Mesh, tid, point);
            return dist.TriangleClosest;
        }

        public static IArray<Vector3d> NearestPoints(this IGeometry g, IArray<Vector3d> points)
        {
            var tree = g.ToG3Sharp().AABBTree();
            return points.Select(tree.NearestPoint);
        }

        public static IGeometry ToIGeometry(this List<DMesh3> meshes)
            => meshes.Select(ToIGeometry).Merge();

        public static IGeometry LoadGeometry(string path)
            => LoadMeshes(path).ToIGeometry();

        public static List<DMesh3> LoadMeshes(string path)
        {
            var builder = new DMesh3Builder();
            var reader = new StandardMeshReader {MeshBuilder = builder};
            var result = reader.Read(path, ReadOptions.Defaults);
            if (result.code == IOCode.Ok)
                return builder.Meshes;
            return null;
        }

        public static void WriteFile(this DMesh3 mesh, string filePath)
            => mesh.WriteFile(filePath, WriteOptions.Defaults);

        public static void WriteFileBinary(this DMesh3 mesh, string filePath)
            => mesh.WriteFile(filePath, new WriteOptions {bWriteBinary = true});

        public static void WriteFileAscii(this DMesh3 mesh, string filePath)
            => mesh.WriteFile(filePath, new WriteOptions { bWriteBinary = false });

       public static void WriteFile(this DMesh3 mesh, string filePath, WriteOptions opts)
        {
            var writer = new StandardMeshWriter();
            var m = new WriteMesh(mesh);
            var result = writer.Write(filePath, new List<WriteMesh> { m }, opts);
            if (!result.Equals(IOWriteResult.Ok))
                throw new Exception($"Failed to write file to {filePath} with result {result.ToString()}");
        }
        public static IArray<Vector3> ToVectors(this DVector<double> self)
        {
            return (self.Length / 3).Select(i => new Vector3((float)self[i * 3], (float)self[i * 3 + 1], (float)self[i * 3 + 2]));
        }

        public static Vector3 ToNumerics(this Vector3d self)
        {
            return new Vector3((float)self.x, (float)self.y, (float)self.z);
        }

        public static IGeometry ToIGeometry(this DMesh3 self)
        {
            self.CompactInPlace();
            var verts = self.Vertices().Select(ToNumerics).ToIArray();
            var indices = self.TrianglesBuffer.ToIArray();
            return verts.TriMesh(indices);
        }

        public static Vector3d ToVector3D(this Vector3 self)
        {
            return new Vector3d(self.X, self.Y, self.Z);
        }

        public static Vector3d ToG3Sharp(this Vector3 self)
        {
            return new Vector3d(self.X, self.Y, self.Z);
        }

        public static DMesh3 ToG3Sharp(this IGeometry self)
        {
            var r = new DMesh3();
            foreach (var v in self.Vertices.ToEnumerable())
                r.AppendVertex(v.ToVector3D());
            var indices = self.ToTriMesh().Indices;
            for (var i = 0; i < indices.Count; i += 3)
            {
                var result = r.AppendTriangle(indices[i], indices[i + 1], indices[i + 2]);
                if (result < 0)
                {
                    if (result == DMesh3.NonManifoldID)
                        throw new Exception("Can't create non-manifold mesh");
                    if (result == DMesh3.InvalidID)
                        throw new Exception("Invalid vertex ID");
                    throw new Exception("Unknown error creating mesh");
                }
            }
            return r;
        }

        public static IGeometry Reduce(this IGeometry self, float percent)
        {
            return self.ToG3Sharp().Reduce(percent).ToIGeometry();
        }
    }
}

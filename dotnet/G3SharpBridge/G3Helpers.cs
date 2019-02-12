using g3;
using System;
using System.Diagnostics;
using System.Numerics;

namespace Ara3D
{
    public static class G3Helpers
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
            r.ReduceToVertexCount((int)target);
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
    }
}

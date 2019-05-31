using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ara3D
{
    public static class SceneExtensions
    {
        public static IGeometry TransformedGeometry(this ISceneNode node)
            => node.Geometry.Transform(node.Transform);

        public static IEnumerable<IGeometry> TransformedGeometries(this IScene scene)
            => scene.AllNodes().Select(TransformedGeometry);

        public static IGeometry ToIGeometry(this IScene scene)
            => scene.TransformedGeometries().Merge();

        public static IEnumerable<ISceneNode> AllNodes(this ISceneNode node)
            => Util.DepthFirstTraversal(node, n => n.Children.ToEnumerable());

        public static IEnumerable<ISceneNode> AllNodes(this IScene scene)
            => scene.Root.AllNodes();

        public static bool HasLoop(this ISceneNode n)
        {
            if (n == null) return false;
            var visited = new HashSet<ISceneNode>();
            for (; n != null; n = n.Parent)
            {
                if (visited.Contains(n))
                    return true;
                visited.Add(n);
            }

            return false;
        }

        // TODO: this is all an issue: generalized merging obliterates instancing. 
        // I need a solution that will allow me to figure out if I am merging things that I have already merged, and keep those things. 
        // If this can work as a post-facto operation it would be best. 

        // OR: I merge instanced geoemtry ... every node that referred to this geometry, will now refer to that geometry. 
        // problem is that merging is a "node" solution, not an instancing solution is it. 
        // Really the solution seem to be for nodes that are instances, to somehow know it, so that when you do something to them, it affects 
        // all of the instances. Which implies a kind of rebuilding of the node graph or something. 

        public static ISceneNode Merge(this IEnumerable<ISceneNode> nodes)
        {
            var list = nodes.ToList();
            if (list.Count == 0) return null;
            if (list.Count == 1) return list[0];
            return list[0].Merge(list.Skip(1));
        }

        public static IGeometry MergedGeometry(this ISceneNode node)
            => node.AllNodes().Select(n => n.Geometry).Merge();

        public static ISceneNode Merge(this ISceneNode node, IEnumerable<ISceneNode> rest)
        {
            var inv = node.Transform.Inverse();
            var geos = rest.Select(n => n.Geometry.Transform(n.Transform * inv)).Prepend(node.Geometry);
            // TODO: keep the node properties? Or merge them all together? Or throw them all out?
            return new SceneNode(node.Properties, geos.Merge(), node.Transform);
        }

        public static ISceneNode MergeWorldSpace(this IEnumerable<ISceneNode> nodes)
            => new SceneNode(nodes.FirstOrDefault()?.Properties, nodes.Select(n => n.TransformedGeometry()).Merge());

        public static IScene ToScene(this IEnumerable<IEnumerable<ISceneNode>> groups)
            => groups.Select(xs => xs.Merge()).ToScene();

        public static Matrix4x4 LocalTransform(this ISceneNode node)
            => node.Parent != null
                ? node.Transform * node.Parent.Transform.Inverse()
                : node.Transform;

        public static IEnumerable<IGeometry> UniqueGeometries(this IScene scene)
            => scene.AllNodes().Select(n => n.Geometry).WhereNotNull().Distinct();

        public static IEnumerable<IGeometry> UntransformedGeometries(this IScene scene)
            => scene.AllNodes().Select(n => n.Geometry);

        public static bool HasGeometry(this IScene scene)
            => scene.AllNodes().Any(n => n.Geometry != null);

        public static string GetName(this ISceneNode n)
            => n.Properties?["name"] ?? "_";

        /// <summary>
        /// Creates a new scene. After calling ToScene, Add can no longer be called.
        /// Tries to recreate the scene tree from the nodes 
        /// </summary>
        public static IScene ToScene(this IEnumerable<ISceneNode> nodes)
        {
            var props = nodes.FirstOrDefault()?.Scene?.Properties;

            var newNodes = new List<ISceneNode> {new SceneNode(null)};
            var geometries = new HashSet<IGeometry>();
            var root = newNodes[0];
            var tmp = new Dictionary<ISceneNode, SceneNode>();

            // Create a new scene node for each node passed in
            foreach (var n in nodes)
            {
                if (n == null)
                    continue;

                var g = n.Geometry;
                var r = new SceneNode(n.Properties, g, n.Transform);
                geometries.Add(g);
                newNodes.Add(r);
                tmp.Add(n, r);
            }

            foreach (var kv in tmp)
            {
                var originalNode = kv.Key;
                var createdNode = kv.Value;

                // Look for a parent, or any ancestor along the path: the node created for it becomes the new parent
                var p = originalNode.Parent;
                while (p != null)
                {
                    if (tmp.ContainsKey(p))
                    {
                        tmp[p]._AddChild(createdNode);
                        break;
                    }

                    p = p.Parent;
                }

                // The parent was not part of the nodes added, s
                if (p == null)
                    (root as SceneNode)._AddChild(createdNode);
            }

            var scene = new Scene(props, newNodes[0]);

            var id = 0;
            foreach (var n in newNodes)
            {
                var sn = n as SceneNode;
                sn.Id = id++;
                sn.Scene = scene;
            }

            return scene;
        }

        public static IScene Filter(this IScene scene, Func<ISceneNode, bool> func)
            => scene.Root.AllNodes().Where(func).ToScene();

        public static int NumNodes(this IScene scene)
            => scene.AllNodes().Count();

        public static IScene SetProperties(this IScene scene, ISceneProperties properties)
            => new Scene(properties, scene.Root);

        public static Dictionary<IGeometry, int> GeometryCounts(this IScene scene)
            => scene.AllNodes().CountInstances(x => x.Geometry);

        public static ISceneNode ReplaceGeometry(this ISceneNode node, IGeometry g)
            => new SceneNode(node.Properties, g, node.Transform);

        public static IScene ReplaceGeometry(this IScene scene, Dictionary<IGeometry, IGeometry> lookup)
            => scene.AllNodes().Select(n => n.ReplaceGeometry(n.Geometry != null ? lookup[n.Geometry] : null))
                .ToScene();
    }
}
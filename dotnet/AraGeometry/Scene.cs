using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ara3D
{
    public interface IScene
    {
        ISceneNode Root { get; }
        IArray<ISceneNode> Nodes { get; }
    }

    public interface ISceneNode
    {
        string Name { get; }
        Matrix4x4 Transform { get; }
        IGeometry Geometry { get; }
        ISceneNode Parent { get; }
        IArray<ISceneNode> Children { get; }
    }

    public class SceneNode : ISceneNode
    {
        public SceneNode(IGeometry g = null, string name = "", Matrix4x4? m = null)
        {
            Transform = m ?? Matrix4x4.Identity;
            Geometry = g;
            Name = name;
        }

        public string Name { get; }
        public Matrix4x4 Transform { get; } 
        public IGeometry Geometry { get; }
        public ISceneNode Parent => _Parent;
        public IArray<ISceneNode> Children => _Children.ToIArray();

        public SceneNode _Parent { get; set; }
        public List<ISceneNode> _Children { get; } = new List<ISceneNode>();

        public void _AddChild(SceneNode n)
        {
            _Children.Add(n);
            n._Parent = this;
            // Slow process: Log(N) operation
            Debug.Assert(!n.HasLoop());
        }
    }

    public class Scene : IScene
    {
        public Scene(ISceneNode root, IArray<IGeometry> geometries, IArray<ISceneNode> nodes)
        {
            Root = root;
            Geometries = geometries;
            Nodes = nodes;
        }
        public ISceneNode Root { get; }
        public IArray<IGeometry> Geometries { get; }
        public IArray<ISceneNode> Nodes { get; }
    }

    public class SceneBuilder
    {
        private readonly List<ISceneNode> _Nodes = new List<ISceneNode> {new SceneNode()};
        private readonly HashSet<IGeometry> _Geometries = new HashSet<IGeometry>();
        private SceneNode _Root => _Nodes[0] as SceneNode;
        private readonly Dictionary<ISceneNode, SceneNode> tmp = new Dictionary<ISceneNode, SceneNode>();
        private Scene _Scene;

        public SceneBuilder(params ISceneNode[] nodes)
            => Add(nodes);

        /// <summary>
        /// Adds a collection of nodes. Cannot be called after the Scene has been built
        /// </summary>
        public SceneBuilder Add(params ISceneNode[] nodes)
        {
            if (_Scene != null)
                throw new Exception("Scene already built");

            // Create a new scene node for each node passed in
            foreach (var n in nodes)
            {
                if (n == null)
                    continue;

                var g = n.Geometry;
                var r = new SceneNode(g, n.Name, n.Transform);
                _Geometries.Add(g);
                _Nodes.Add(r);
                tmp.Add(n, r);
            }

            return this;
        }

        /// <summary>
        /// Creates a new scene. After calling ToScene, Add can no longer be called. 
        /// </summary>
        public IScene ToScene()
        {
            if (_Scene != null)
                return _Scene;
                
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
                    _Root._AddChild(createdNode);
            }
            // TODO: validate if lock is needed  
            return _Scene = new Scene(_Nodes[0], _Geometries.ToIArray(), _Nodes.ToIArray());
        }
    }

    public static class SceneExtensions
    {
        public static IGeometry TransformedGeometry(this ISceneNode node)
            => node.Geometry.Transform(node.Transform);

        public static IArray<IGeometry> TransformedGeometries(this IScene scene)
            => scene.Nodes.Select(TransformedGeometry);

        public static IGeometry ToIGeometry(this IScene scene)
            => scene.TransformedGeometries().Merge();

        public static IEnumerable<ISceneNode> AllNodes(this ISceneNode node)
            => Util.DepthFirstTraversal(node, n => n.Children.ToEnumerable());

        public static ISceneNode Root(this IScene scene)
            => scene.Nodes.Count >= 0 ? scene.Nodes[0] : null;

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


        public static IScene ToScene(this ISceneNode[] nodes)
            => new SceneBuilder(nodes).ToScene();

        public static IScene ToScene(this IEnumerable<ISceneNode> nodes)
            => nodes.ToArray().ToScene();

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

        public static ISceneNode Merge(this ISceneNode node, IEnumerable<ISceneNode> rest)
        {
            var inv = node.Transform.Inverse();
            var geos = rest.Select(n => n.Geometry.Transform(n.Transform * inv)).Prepend(node.Geometry);
            return new SceneNode(geos.Merge(), node.Name, node.Transform);
        }

        public static ISceneNode MergeWorldSpace(this IEnumerable<ISceneNode> nodes)
            => new SceneNode(nodes.Select(n => n.TransformedGeometry()).Merge());

        public static IScene ToScene(this IEnumerable<IEnumerable<ISceneNode>> groups)
            => groups.Select(Merge).ToScene();

        public static Matrix4x4 LocalTransform(this ISceneNode node) 
            => node.Parent != null 
                ? node.Transform * node.Parent.Transform.Inverse() 
                : node.Transform;

        public static IEnumerable<IGeometry> UniqueGeometries(this IScene scene)
            => scene.AllNodes().Select(n => n.Geometry).Distinct();

        public static IEnumerable<IGeometry> UntransformedGeometries(this IScene scene)
            => scene.AllNodes().Select(n => n.Geometry);

        public static bool HasGeometry(this IScene scene)
            => scene.AllNodes().Any(n => n.Geometry != null);
    }
}

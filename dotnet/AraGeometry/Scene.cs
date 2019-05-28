using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ara3D
{
    /// <summary>
    /// An IScene is a generic representation of a 3D scene graph and associated meta-data.
    /// TODO: the properties lookups are going to be really slow and painful. 
    /// </summary>
    public interface IScene
    {
        ISceneNode Root { get; }
        IArray<ISceneNode> Nodes { get; }
        IArray<IGeometry> Geometries { get; }
        ISceneProperties Properties { get; }
    }

    /// <summary>
    /// For different types of objects in the scene (e.g. nodes, face groups, elements, etc.)
    /// there are individual property sets. A property set might just be name, or something else.
    /// Some property sets have a schema, meaning that the keys are guaranteed to be there and have a
    /// specific types. Some are unstructured string pairs.
    /// It is unclear whether I should store analyses in this as well.
    /// </summary>
    public interface ISceneProperties : IMap<string, IPropertiesMap>
    { }

    /// <summary>
    /// A property set is a mapping from an ID to a set of properties. 
    /// </summary>
    public interface IPropertiesMap : IMap<int, IProperties>
    { }

    /// <summary>
    /// These are the common property sets found in 3D scenes. 
    /// </summary>
    public enum CorePropertyTypes
    {
        Scene,
        Node,
        Geometry,
        FaceGroup,
        Material,
        Camera,
        Layer,
    }

    /// <summary>
    /// These are common property sets found in scenes generated from Revit data.
    /// </summary>
    public enum RevitPropertyTypes
    {
        RevitFace,
        RevitElement,
        RevitFamily,
        RevitFamilyType,
        RevitFamilyInstance, 
        RevitView,
        RevitRoom,
        RevitPhase,
        RevitDocument,
        RevitMaterial,
    }

    /// <summary>
    /// A node in a scene graph. 
    /// </summary>
    public interface ISceneNode
    {
        int Id { get; }
        IScene Scene { get; }
        Matrix4x4 Transform { get; }
        IGeometry Geometry { get; }
        ISceneNode Parent { get; }
        IArray<ISceneNode> Children { get; }
    }

    /// <summary>
    /// Lookup table: mapping from a key to some value.
    /// TODO: this should be in LinqArray : an array is a special case of an ILookup
    /// </summary>
    public interface IMap<TKey, TValue>
    {
        IArray<TKey> Keys { get; }
        IArray<TValue> Values { get; } 
        TValue this[TKey key] { get; }
    }

    /// <summary>
    /// A lookup table from strings to strings.
    /// TODO: for numerical types I want a parse function (e.g. Box(Vector3(0, 0, 0), Vector3(1, 1, 1))
    /// </summary>
    public interface IStringMap : IMap<string, string>
    { }

    /// <summary>
    /// Properties are a special type of string lookup table
    /// TODO: introduce the concept of schemas to help with parsing. 
    /// </summary>
    public interface IProperties : IStringMap
    { }

    /// <summary>
    /// 
    /// </summary>
    public class SceneProperties : ISceneProperties
    {
        public IArray<string> Keys => LinqArray.Empty<string>();
        public IArray<IPropertiesMap> Values => LinqArray.Empty<IPropertiesMap>();
        public IPropertiesMap this[string key] => null;
    }

    public class SceneNode : ISceneNode
    {
        public SceneNode(IGeometry g = null, Matrix4x4? m = null)
        {
            Transform = m ?? Matrix4x4.Identity;
            Geometry = g;
        }

        // TODO: set this in the builder 
        public IScene Scene { get; set; }
        // TODO: set this in the builder 
        public int Id { get; set; }

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

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PropertyRelation
    {
        public int Id { get; }
        public int KeyIndex { get; }
        public int ValueIndex { get; }
    }

    public class Scene : IScene
    {
        public Scene(ISceneProperties properties, ISceneNode root, IArray<IGeometry> geometries, IArray<ISceneNode> nodes)
        {
            Root = root;
            Geometries = geometries;
            Nodes = nodes;
            Properties = properties;
        }

        public ISceneNode Root { get; }
        public IArray<IGeometry> Geometries { get; }
        public IArray<ISceneNode> Nodes { get; }
        public ISceneProperties Properties { get; }
    }

    public class Properties : IProperties
    {
        public IArray<string> Keys { get; }
        public IArray<string> Values { get; }
        public string this[string key]  => Values[Keys.IndexOf(key)];
        
        public Properties(string input)
        {
            var kvs = new List<Tuple<string, string>>();
            foreach (var kv in input.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(kv))
                    continue;
                var eq = input.IndexOf('=');
                if (eq >= 0)
                {
                    kvs.Add(Tuple.Create(kv.Substring(0, eq), kv.Substring(eq + 1)));
                }
                else
                {
                    kvs.Add(Tuple.Create(kv, ""));
                }
            }

            kvs.Sort();
            Keys = kvs.Select(kv => kv.Item1).ToIArray();
            Values = kvs.Select(kv => kv.Item2).ToIArray();
        }
    }

    public class EmptyProperties : IProperties
    {
        public IArray<string> Keys => LinqArray.Empty<string>();
        public IArray<string> Values => LinqArray.Empty<string>();
        public string this[string key] => "";
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
            // TODO: keep the node properties? Or merge them all together? Or throw them all out?
            return new SceneNode(geos.Merge(), node.Transform);
        }

        public static ISceneNode MergeWorldSpace(this IEnumerable<ISceneNode> nodes)
            => new SceneNode(nodes.Select(n => n.TransformedGeometry()).Merge());

        public static IScene ToScene(this IEnumerable<IEnumerable<ISceneNode>> groups)
            => groups.Select(xs => xs.Merge()).ToScene();

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

        public static string GetName(this ISceneNode n)
            => n.Scene.Properties["Node"][n.Id]["Name"];

        /// <summary>
        /// Creates a new scene. After calling ToScene, Add can no longer be called.
        /// Tries to recreate the scene tree from the nodes 
        /// </summary>
        public static IScene ToScene(this IEnumerable<ISceneNode> nodes)
        {
            var _Nodes = new List<ISceneNode> { new SceneNode() };
            var _Geometries = new HashSet<IGeometry>();
            var _Root = _Nodes[0];
            var tmp = new Dictionary<ISceneNode, SceneNode>();

            // Create a new scene node for each node passed in
            foreach (var n in nodes)
            {
                if (n == null)
                    continue;

                var g = n.Geometry;
                var r = new SceneNode(g, n.Transform);
                _Geometries.Add(g);
                _Nodes.Add(r);
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
                    (_Root as SceneNode)._AddChild(createdNode);
            }

            // TODO: validate if lock is needed  
            // TODO: do something about the SceneProperties ... if they are coming along for the ride we should keep them.
            var _Scene = new Scene(new SceneProperties(), _Nodes[0], _Geometries.ToIArray(), _Nodes.ToIArray());

            var id = 0;
            foreach (var n in _Nodes)
            {
                var sn = n as SceneNode;
                sn.Id = id++;
                sn.Scene = _Scene;
            }

            return _Scene;
        }
    }
}

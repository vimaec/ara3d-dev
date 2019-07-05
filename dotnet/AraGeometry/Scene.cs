using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ara3D
{
    public class PropertiesLookup : LookupFromDictionary<int, IProperties>, IPropertiesLookup
    {
        public PropertiesLookup(Dictionary<int, IProperties> d = null)
            : base(d ?? new Dictionary<int, IProperties>())
        { }

        public static PropertiesLookup Empty = new PropertiesLookup();
    }   

    public class SceneNode : ISceneNode
    {
        public SceneNode(IProperties props, IGeometry g = null, Matrix4x4? m = null)
        {
            Transform = m ?? Matrix4x4.Identity;
            Geometry = g;
            Properties = props ?? new EmptyProperties();
        }

        public IScene Scene { get; set; }
        public int Id { get; set; }
        public IProperties Properties { get; set;}
         
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
            //Debug.Assert(!n.HasLoop());
        }
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class PropertyRelation
    {
        public int Id { get; }
        public int KeyIndex { get; }
        public int ValueIndex { get; }
    }

    public class SceneProperties : ISceneProperties
    {
        public SceneProperties(ILookup<string, IPropertiesLookup> properties)
        {
            Scene = AllProperties.GetOrDefault("Scene") ?? PropertiesLookup.Empty;
            Materials = AllProperties.GetOrDefault("Material") ?? PropertiesLookup.Empty;
            Surfaces = AllProperties.GetOrDefault("Surface") ?? PropertiesLookup.Empty;
            Nodes = AllProperties.GetOrDefault("Node") ?? PropertiesLookup.Empty;
            Geometries = AllProperties.GetOrDefault("Geometry") ?? PropertiesLookup.Empty;
            Objects = AllProperties.GetOrDefault("Object") ?? PropertiesLookup.Empty;
        }

        public ILookup<string, IPropertiesLookup> AllProperties { get; }

        public IPropertiesLookup Scene { get; }
        public IPropertiesLookup Materials { get; }
        public IPropertiesLookup AssetProperties { get; }
        public IPropertiesLookup Surfaces { get; }
        public IPropertiesLookup Nodes { get; }
        public IPropertiesLookup Geometries { get; }
        public IPropertiesLookup Objects { get; }

        public static SceneProperties Empty = new SceneProperties(new EmptyLookup<string, IPropertiesLookup>());
    }

    public class Scene : IScene
    {
        public Scene(
            ISceneNode root,
            ISceneProperties properties,
            IArray<ISceneNode> nodes = null, 
            IArray<IGeometry> geometries = null,
            IArray<ISurfaceRelation> surfaces = null
            )
        {
            Root = root;
            Nodes = nodes;
            Geometries = geometries;
            Surfaces = surfaces;
            Properties = properties;

            // TODO: compute the nodes, geometries and surfaces if absent
        }

        public ISceneNode Root { get; }
        public ISceneProperties Properties { get; }
        public IArray<ISceneNode> Nodes { get; }
        public IArray<IGeometry> Geometries { get; }
        public IArray<ISurfaceRelation> Surfaces { get; }
    }

    public class Properties : LookupFromDictionary<string, string>, IProperties
    {
        public Properties(Dictionary<string, string> d = null)
            : base(d ?? new Dictionary<string, string>())
        {  }

        public static EmptyProperties Empty = new EmptyProperties();
    }

    public class EmptyProperties : EmptyLookup<string, string>, IProperties
    { }
}

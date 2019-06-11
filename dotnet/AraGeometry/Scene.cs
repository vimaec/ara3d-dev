using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ara3D
{
    public class PropertiesLookup : LookupFromDictionary<int, IProperties>, IPropertiesLookup
    {
        public PropertiesLookup(Dictionary<int, IProperties> d)
            : base(d)
        { }
    }

    public class SceneProperties : LookupFromDictionary<string, ILookup<int, string>>, ISceneProperties
    {
        public SceneProperties()
            : base(new Dictionary<string, ILookup<int, string>>())
        { }
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

    public class Scene : IScene
    {
        public Scene(ISceneProperties properties, ISceneNode root)
        {
            Root = root;
            Properties = properties;
        }

        public ISceneNode Root { get; }
        public ISceneProperties Properties { get; }
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

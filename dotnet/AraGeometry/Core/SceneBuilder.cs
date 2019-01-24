using System.Collections.Generic;
using System.Numerics;

namespace Ara3D
{
    public class SceneBuilder
    {
    }

    public class Scene
    {
        public readonly IArray<SceneNode> Nodes;
        public readonly IArray<GeometryNode> Geometries;
        public readonly IArray<PropertySet> PropertySets;
    }

    public class SceneNode
    {
        public readonly int Id;
        public readonly int ParentId;
        public readonly Matrix4x4 GlobalTransform;
        public readonly int GeometryId;
        public readonly int MaterialId;
        public readonly int PropertySetId;
    }

    public class PropertySet
    {
        public readonly IDictionary<string, string> Dictionary = new Dictionary<string, string>();
    }

    public class GeometryNode 
    {
        public readonly int Id;
        public readonly IGeometry Geometry;
        public Box BoundingBox { get; }
    }
}

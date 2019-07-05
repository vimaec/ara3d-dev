namespace Ara3D
{
    /// <summary>
    /// An IScene is a generic representation of a 3D scene graph and associated meta-data.
    /// This is a minimal representation of a scene graph but it is not very efficient. 
    /// Normally you would use a Scene object. 
    /// </summary>
    public interface IScene
    {
        ISceneNode Root { get; }
        ISceneProperties Properties { get; }
        IArray<ISceneNode> Nodes { get; }
        IArray<IGeometry> Geometries { get; }
        IArray<ISurfaceRelation> Surfaces { get; }
        IArray<INamedBuffer> Assets { get; }
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
        IProperties Properties { get; }
    }

    /// <summary>
    /// A property set is a mapping from an ID to a set of properties. 
    /// </summary>
    public interface IPropertiesLookup : ILookup<int, IProperties>
    { }

    /// <summary>
    /// Properties are a special type of string lookup table
    /// TODO: introduce the concept of schemas to help with parsing, validating, constructing, and efficient queries.
    /// </summary>
    public interface IProperties : ILookup<string, string>
    { }

    /// <summary>
    /// Represents the relationship between a surface and an object and material.
    /// A surface is a set of polygons in a mesh that represent a portion of a mesh, usually contiguous.
    /// In Revit this is called a "Face". This might be identified as a smoothing group in 3ds Max. 
    /// https://en.wikipedia.org/wiki/Polygon_mesh. 
    /// When combined with the G3D a "Surface IDs" array associated with each face 
    /// one can omit object id and material id attributes. 
    /// The mesh of a "surface" is defined as all geometries in a scene with the given surface ID. 
    /// </summary>
    public interface ISurfaceRelation
    {
        int SurfaceId { get; }
        int ObjectId { get; }
        int MaterialId { get; }
    }

    /// <summary>
    /// This interface provides accellerated access to common scene properties. 
    /// There are different types of properties and they are relationships 
    /// between integers and strings. 
    /// Properties can be associated with anything, but most commonly
    /// they are materials, surfaces, nodes, geometries, and objects.
    /// </summary>
    public interface ISceneProperties
    {
        ILookup<string, IPropertiesLookup> AllProperties { get; }
        IPropertiesLookup Materials { get; }
        IPropertiesLookup Surfaces { get; }
        IPropertiesLookup Nodes { get; }
        IPropertiesLookup Geometries { get; }
        IPropertiesLookup Objects { get; }
    } 
}
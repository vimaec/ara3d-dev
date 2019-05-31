namespace Ara3D
{
    /// <summary>
    /// An IScene is a generic representation of a 3D scene graph and associated meta-data.
    /// TODO: the properties lookups are going to be really slow and painful. 
    /// </summary>
    public interface IScene
    {
        ISceneNode Root { get; }
        ISceneProperties Properties { get; }
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
    /// For different types of objects in the scene (e.g. nodes, face groups, elements, etc.)
    /// there are individual property sets. A property set might just be name, or something else.
    /// Some property sets have a schema, meaning that the keys are guaranteed to be there and have a
    /// specific types. Some are unstructured string pairs.
    /// It is unclear whether I should store analyses in this as well.
    /// </summary>
    public interface ISceneProperties : ILookup<string, ILookup<int, string>>
    { }

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
}
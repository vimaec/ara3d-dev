using System;
using System.Collections.Generic;

namespace Ara3D
{
    /// <summary>
    /// A surface or smoothing group is a set of polygons in a mesh that represent a portion of a mesh, usually contiguous.
    /// In Revit this is called a "Face". This might be identified as a smoothing group in 3ds Max. 
    /// https://en.wikipedia.org/wiki/Polygon_mesh. A surface will have the same material id. 
    /// </summary>
    public interface ISurface
    {
        int ObjectId { get; }
        int MaterialId { get; }
    }

    /// <summary>
    /// An IScene is a generic representation of a 3D scene graph and associated meta-data.
    /// </summary>
    public interface IScene
    {
        ISceneNode Root { get; }
        ILookup<string, IPropertiesLookup> AllProperties { get; }
    }

    /// <summary>
    /// This is the next generation of scene graphs
    /// </summary>
    public interface IScene2 : IScene
    {
        ISceneNode[] Nodes { get; }
        IGeometry[] Geometries { get; }
        Memory<byte>[] Assets { get; }
        ISurface[] Surfaces { get; }

        IPropertiesLookup MaterialProperties { get; }
        IPropertiesLookup AssetProperties { get; }
        IPropertiesLookup SurfaceProperties { get; }
        IPropertiesLookup NodeProperties { get; }
        IPropertiesLookup GeometryProperties { get; }
        IPropertiesLookup ObjectProperties { get; }
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
}
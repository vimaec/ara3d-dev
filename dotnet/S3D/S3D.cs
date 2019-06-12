/*
    S3D - 3D Scene data format
    Copyright 2018, Ara 3D, Inc.
    Usage licensed under terms of MIT License
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ara3D
{
    public class CommonSceneProperties
    {

    }

    public class Document : IScene
    {
        public IArray<ISurface> Surfaces { get; }
        public IArray<IGeometry> Geometries { get; }
        public ISceneNode Root { get; }
        public List<string> Strings { get; }

        public ISceneProperties Properties { get; }

        /*
        public IPropertiesLookup MaterialProperties { get; }
        public IPropertiesLookup SceneProperties { get; }
        public IPropertiesLookup SurfaceProperties { get; }
        public IPropertiesLookup NodeProperties { get; }
        public IPropertiesLookup GeometryProperties { get; }
        public IPropertiesLookup ObjectProperties { get; }
        */

        // What about Family / Phase / Rooms and other things. 
    }

    public class SerializableDocument
    {
    }

    /// <summary>
    /// A surface or smoothing group is a set of polygons in a mesh that represent a portion of a mesh, usually contiguous.
    /// In Revit this is called a "Face". This might be identified as a smoothing group in 3ds Max. 
    /// https://en.wikipedia.org/wiki/Polygon_mesh. A surface will have the same material id. 
    /// </summary>
    public interface ISurface
    {
        int SurfaceId { get; }
        int ObjectId { get; }
        int MaterialId { get; }
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SerializableNode
    {
        public int GeometryIndex;
        public int InstanceIndex;
        public int ParentIndex;
        public Matrix4x4 WorldTransform;
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SerializableSurface
    {
        public int MaterialId;
        public int ObjectId;
    }

    public static class S3D
    {
    }
}

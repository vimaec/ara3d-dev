/*
    S3D - 3D Scene data format
    Copyright 2018, Ara 3D, Inc.
    Usage licensed under terms of MIT License

    The S3D data format is based on the BFAST format. The first BFAST array contains the names of each of the buffers. 
    The buffers are named as follows, and can occur in any order. 

    * meta.json - File meta-information. Basic information about the file author and version as a JSON object. 
    * nodes.array - A binary array of serializable node structures. 
    * geometries.bfast - A BFAST containing geometry representations
    * surfaces.array - An array serializable surfaces 
    * properties.bfast - A BFAST containing named property tables. The following are expected named tables, but more can be present. A property table i
        * material
        * scene
        * surface
        * node
        * geometry
        * object
    * strings.data - An array of all of the strings used in the properties tables.
    * assets.bfast - Binary assets in a named BFAST
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ara3D
{    
    public class S3D : IScene
    {
        public S3D(SerializableNode[] nodes, G3D[] geometries, SerializableSurface[] surfaces, Memory<byte>[] assets, ILookup<string, ILookup<int, IProperties>> properties, string[] strings)
        {

        }

        public S3D(ILookup<string, string> meta, ISceneNode root, ISceneNode[] nodes, IGeometry[] geometries, ISurface[] surfaces, Memory<byte>[] assets, 
            ILookup<string, IPropertiesLookup> properties, string[] strings)
        {
            Root = root;
            Nodes = nodes;
            Geometries = geometries;
            Surfaces = surfaces;
            Assets = assets;
            AllProperties = properties;
            Strings = strings;

            MaterialProperties = AllProperties.GetOrDefault("Material") ?? PropertiesLookup.Empty;
            SceneProperties = AllProperties.GetOrDefault("Scene") ?? PropertiesLookup.Empty;
            SurfaceProperties = AllProperties.GetOrDefault("Surface") ?? PropertiesLookup.Empty;
            NodeProperties = AllProperties.GetOrDefault("Node") ?? PropertiesLookup.Empty;
            GeometryProperties = AllProperties.GetOrDefault("Geometry") ?? PropertiesLookup.Empty;
            ObjectProperties = AllProperties.GetOrDefault("Object") ?? PropertiesLookup.Empty;            

            // TODO: validate the material IDs, surface IDs, etc. 
        }

        public ILookup<string, string> Meta { get; }
        public ISceneNode Root { get; }
        public ISceneNode[] Nodes { get; }
        public IGeometry[] Geometries { get; }
        public ISurface[] Surfaces { get; }
        public Memory<byte>[] Assets { get; }

        public string[] Strings { get; }

        public ILookup<string, IPropertiesLookup> AllProperties { get; }

        public IPropertiesLookup MaterialProperties { get; }
        public IPropertiesLookup SceneProperties { get; }
        public IPropertiesLookup SurfaceProperties { get; }
        public IPropertiesLookup NodeProperties { get; }
        public IPropertiesLookup GeometryProperties { get; }
        public IPropertiesLookup ObjectProperties { get; }
    }

    public static class S3DExtension
    {
        public static void Write(IScene2 scene, string filePath)
        {
            // Converts the thing
            // Creates a BFAST with a manifest 
            // Geometry

            // We have assets 

            // A section for all of the properties 

            // File
        }

        public static IScene2 Read(string filePath)
        {
            throw new NotImplementedException();
        }
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

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SerializableProperty
    {
        public int ItemId;
        public int KeyId;
        public int ValueId;
    }
}

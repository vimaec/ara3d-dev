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
    * material.properties.bin - A BFAST containing named property tables. The following are expected named tables, but more can be present. A property table i
        * material
        * scene
        * surface
        * node
        * geometry
        * object
    * strings.data - An array of all of the strings used in the properties tables.
    * assets.bfast - Binary assets in a named BFAST
     
    There S3D format has the following concepts of elements, each of which can have properties:
        * nodes
        * geometry
        * surface
        * object 
        * material 
       
     These different types of elements are identified by a 32-bit integer. For nodes and geometries
     the id of these elements is implied by the order in which they appear in the input array. 
     For other element (e.g. surface / object / material) the ID is stored in the data struct 
     itself.      
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;

namespace Ara3D
{   
    /// <summary>
    /// Question: does this store information in a more "serializable" format and do the work in the
    /// constructor, or something else? 
    /// </summary>
    public class S3D : IScene
    {
        public S3D(ILookup<string, string> meta, 
            ISceneNode root, 
            ISceneNode[] nodes, 
            IGeometry[] geometries, 
            ISurface[] surfaces, 
            Memory<byte>[] assets, 
            ILookup<string, IPropertiesLookup> properties)
        {
            Meta = meta;
            Root = root;
            Nodes = nodes ?? new ISceneNode[0];
            Geometries = geometries ?? new IGeometry[0];
            Surfaces = surfaces ?? new ISurface[0];
            Assets = assets ?? new Memory<byte>[0];
            AllProperties = properties ?? new EmptyLookup<string, IPropertiesLookup>();

            MaterialProperties = AllProperties.GetOrDefault("Material") ?? PropertiesLookup.Empty;
            SceneProperties = AllProperties.GetOrDefault("Scene") ?? PropertiesLookup.Empty;
            SurfaceProperties = AllProperties.GetOrDefault("Surface") ?? PropertiesLookup.Empty;
            NodeProperties = AllProperties.GetOrDefault("Node") ?? PropertiesLookup.Empty;
            GeometryProperties = AllProperties.GetOrDefault("Geometry") ?? PropertiesLookup.Empty;
            ObjectProperties = AllProperties.GetOrDefault("Object") ?? PropertiesLookup.Empty;            
        }

        public ILookup<string, string> Meta { get; }
        public ISceneNode Root { get; }
        public ISceneNode[] Nodes { get; }
        public IGeometry[] Geometries { get; }
        public ISurface[] Surfaces { get; }
        public Memory<byte>[] Assets { get; }

        public ILookup<string, IPropertiesLookup> AllProperties { get; }

        public IPropertiesLookup MaterialProperties { get; }
        public IPropertiesLookup SceneProperties { get; }
        public IPropertiesLookup SurfaceProperties { get; }
        public IPropertiesLookup NodeProperties { get; }
        public IPropertiesLookup GeometryProperties { get; }
        public IPropertiesLookup ObjectProperties { get; }
    }

    public static class S3DExtensions
    {
        public const string SectionNameMetaJson = "meta.json";
        public const string SectionNameNodesArray = "nodes.array";
        public const string SectionNameGeometriesBfast = "geometries.bfast";
        public const string SectionNameSurfacesArray = "surfaces.array";
        public const string SectionNamePropertiesBFast = "properties.bfast";
        public const string SectionNameStringData = "strings.data";
        public const string SectionNameAssetsBfast = "assets.bfast";

        public static string ToJsonString(this ILookup<string, string> lookup)
            => "{" + lookup.Keys.Select(k => $"{k.Quoted()}:{lookup[k].Quoted()}" + "}");

        public static SerializableNode CreateSerializableNode(int geometry, int parent, Matrix4x4 matrix)
             => new SerializableNode { GeometryIndex = geometry, InstanceIndex = -1, ParentIndex = parent, WorldTransform = matrix };

        public static SerializableSurface ToSerializableSurface(this ISurface surface)
            => new SerializableSurface { MaterialId = surface.MaterialId, ObjectId = surface.ObjectId, SurfaceId = surface.SurfaceId };

        public static SerializableProperty CreateSerializableProperty(int elementId, string key, string value, IndexedSet<string> strings)
            => new SerializableProperty { ItemId = elementId, KeyStringId = strings.Add(key), ValueStringId = strings.Add(value) };

        public static IEnumerable<SerializableProperty> CreateSerializableProperties(int elementId, IProperties props, IndexedSet<string> strings)
            => props.Keys.ToEnumerable().Select(k => CreateSerializableProperty(elementId, k, props[k], strings));

        public static IEnumerable<SerializableProperty> ToSerializableProperties(this IPropertiesLookup props, IndexedSet<string> strings)
            => props.Keys.ToEnumerable().SelectMany(k => CreateSerializableProperties(k, props[k], strings));

        public static List<byte[]> ToBFastBuffers<T>(this Dictionary<string, T[]> dict) where T: struct
        {
            var r = new List<byte[]>();
            var names = dict.Keys.JoinWithNull();
            r.Add(names.ToBytesUtf8());
            r.AddRange(dict.Values.Select(v => BFast.ToBytes(v)));
            return r;
        }

        public static void WriteToFolder(this S3D scene, string folder)
        {
            // Write the meta-data 
            File.WriteAllText(Path.Combine(folder, SectionNameMetaJson), scene.Meta.ToJsonString());

            // Compute the node lookup and the list of nodes 
            var nodeLookup = scene.Nodes.ToIndexedSet();
            var nodes = nodeLookup.ToList();

            // Compute the geomery lookup and the list of geometries 
            var geometryLookup = scene.GeometryLookup();
            var geometries = geometryLookup.ToList();

            // Compute the list of serializable nodes
            var serializableNodes = nodes.Select(n =>
                CreateSerializableNode(
                    n.Geometry == null ? -1 : geometryLookup[n.Geometry],
                    n.Parent == null ? -1 : nodeLookup[n.Parent],
                    n.Transform)).ToArray();
                        
            // Write the nodes 
            Util.WriteFixedLayoutClassList(Path.Combine(folder, SectionNameNodesArray), serializableNodes);

            // Get the geometry bytes and write them all to file.
            var g3ds = geometries.Select(g => g.ToG3D().ToBytes());
            g3ds.ToBFastFile(Path.Combine(folder, SectionNameGeometriesBfast));

            // surfaces.array - An array serializable surfaces
            var serializableSurfaces = scene.Surfaces.Select(ToSerializableSurface).ToArray();
            Util.WriteFixedLayoutClassList(Path.Combine(folder, SectionNameSurfacesArray), serializableSurfaces);
            
            // Create a new string table
            var localStrings = new IndexedSet<string>();

            // Get all properties 
            var propsDictionary = new Dictionary<string, SerializableProperty[]>();
            foreach (var k in scene.AllProperties.Keys.ToEnumerable())
            {
                var props = scene.AllProperties[k].ToSerializableProperties(localStrings);
                propsDictionary.Add(k, props.ToArray());                          
            }
            
            // TODO: write the props

            // Write the string data 
            var stringData = string.Join("\0", localStrings.Values);
            File.WriteAllText(Path.Combine(folder, SectionNameStringData), stringData);

            // Write the assets 
            scene.Assets.ToBFastFile(Path.Combine(folder, SectionNameAssetsBfast));
        }

        public static IScene2 Read(string filePath)
        {
            throw new NotImplementedException();
        }

        public static S3D ToS3D(this IScene scene)
        {
            var meta = new Dictionary<string, string> {
                { "version", "1.0.0" },
                { "filetype", "s3d" } }.ToLookup();
            var nodes = scene.AllNodes().ToArray();
            var geometries = scene.AllDistinctGeometries().ToArray();
            var surfaces = new ISurface[0];
            var assets = new Memory<byte>[0];
            var props = scene.AllProperties;
            return new S3D(meta, scene.Root, nodes, geometries, surfaces, assets, props);
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
        public int SurfaceId;
        public int MaterialId;
        public int ObjectId;
    }

    [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class SerializableProperty
    {
        public int ItemId;
        public int KeyStringId;
        public int ValueStringId;
    }
}

/*
    S3D - 3D Scene data format
    Copyright 2018, Ara 3D, Inc.
    Usage licensed under terms of MIT License

    The S3D data format is an efficient and generic data format for representing 3D scene data and associated 
    assets. 
    
    The first BFAST array contains the names of each of the buffers. The buffers are named as follows, 
    and can occur in any order:

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
    /// WIP: Document versioning and meta-information. 
    /// This should provide enough information to track where a document came from, and given
    /// a URN we should be able to use a resolver to look up in a database find the S3D and the source documents. 
    /// Note that an S3D might be exported from a source document (say a RVT) but still be considered 
    /// a revision of a previous S3D exported from an earlier version of that Revit document. So 
    /// the previous S3D was used track changes between the RVT and a previous export. 
    /// </summary>
    public class S3DMetaData
    {
        /// <summary>
        /// Unique identifier of the current S3D scene
        /// </summary>
        public string Guid;
        
        /// <summary>
        /// The date time that the S3D was created. 
        /// </summary>
        public DateTime DateTime;
        
        /// <summary>
        /// Version of the S3D document. 
        /// </summary>
        public Version DocumentVersion;
        
        /// <summary>
        /// Organization name: should be unique. 
        /// </summary>
        public string Organization;
        
        /// <summary>
        /// Project name.
        /// </summary>
        public string Project;
        
        /// <summary>
        /// Optional name of the document 
        /// </summary>
        public string DocumentName;
        
        /// <summary>
        /// Version of the S3D format 
        /// </summary>
        public Version S3DVersion;

        /// <summary>
        /// A computed URN the identifies this document. 
        /// s3d:filetype:organization:project-name:document-name:date-time:major:minor:revision:guid
        /// </summary>
        public string URN;

        /// <summary>
        /// A URN that identifies the document that was used as a source for this document
        /// this might be a URN of an S3D, or it might be another file that the S3D 
        /// was exported from.
        /// </summary>
        public string SourceDocumentURN;

        /// <summary>
        /// If this a revision of an S3D, this URN identifies the S3D that was the parent revision
        /// </summary>
        public string S3DParentURN;

        /// <summary>
        /// If this a revision of an S3D, this URN identifies the S3D that was the original version.
        /// </summary>
        public string OriginS3DURN;

        /// <summary>
        /// Information about who this document belongs to any copyright notice. 
        /// </summary>
        public string Attribution;
    }

    /// <summary>
    /// An S3D class is a serializable container for scene data and assets.
    /// </summary>
    public class S3D 
    {
        public string Meta { get; set; }
        public SerializableNode Root { get; set; }
        public SerializableNode[] Nodes { get; set; } = new SerializableNode[0];
        public IG3D[] Geometries { get; set; } = new IG3D[0];
        public SerializableSurface[] Surfaces { get; set; } = new SerializableSurface[0];
        public SerializableProperty[] Properties { get; set; } = new SerializableProperty[0];
        public string[] StringData { get; set; } = new string[0];
        public INamedBuffer[] Assets { get; set; } = new INamedBuffer[0];

        public const string DefaultMeta = "{ \"version\": \"1.0.0\", \"filetype\", \"s3d\" }";
        public const string SectionNameMetaJson = "meta.json";
        public const string SectionNameNodesArray = "nodes.array";
        public const string SectionNameGeometriesBfast = "geometries.bfast";
        public const string SectionNameSurfacesArray = "surfaces.array";
        public const string SectionNamePropertiesBFast = "properties.bfast";
        public const string SectionNameStringData = "strings.data";
        public const string SectionNameAssetsBfast = "assets.bfast";
    }

    /// <summary>
    /// Implements 
    /// </summary>
    public class S3DScene: IScene
    {
        public S3DScene(S3D data)
        {
            // TODO:
        }

        public ISceneNode Root => throw new NotImplementedException();
        public ISceneProperties Properties => throw new NotImplementedException();
        public IArray<ISceneNode> Nodes => throw new NotImplementedException();
        public IArray<IGeometry> Geometries => throw new NotImplementedException();
        public IArray<ISurfaceRelation> Surfaces => throw new NotImplementedException();
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

    public static class S3DExtensions
    {
        /*
        ILookup<string, string> lookup)
            => "{" + lookup.Keys.Select(k => $"{k.Quoted()}:{lookup[k].Quoted()}" + "}");
        */

        public static SerializableNode CreateSerializableNode(int geometry, int parent, Matrix4x4 matrix)

             => new SerializableNode { GeometryIndex = geometry, InstanceIndex = -1, ParentIndex = parent, WorldTransform = matrix };

        public static SerializableSurface ToSerializableSurface(this ISurfaceRelation surface)
            => new SerializableSurface { MaterialId = surface.MaterialId, ObjectId = surface.ObjectId, SurfaceId = surface.SurfaceId };

        public static SerializableProperty CreateSerializableProperty(int elementId, string key, string value, IndexedSet<string> strings)
            => new SerializableProperty { ItemId = elementId, KeyStringId = strings.Add(key), ValueStringId = strings.Add(value) };

        public static IEnumerable<SerializableProperty> CreateSerializableProperties(int elementId, IProperties props, IndexedSet<string> strings)
            => props.Keys.ToEnumerable().Select(k => CreateSerializableProperty(elementId, k, props[k], strings));

        public static IEnumerable<SerializableProperty> ToSerializableProperties(this IPropertiesLookup props, IndexedSet<string> strings)
            => props.Keys.ToEnumerable().SelectMany(k => CreateSerializableProperties(k, props[k], strings));

        public static List<byte[]> ToBFastBuffers<T>(this Dictionary<string, T[]> dict) where T : struct
        {
            var r = new List<byte[]>();
            var names = dict.Keys.JoinWithNull();
            r.Add(names.ToBytesUtf8());
            r.AddRange(dict.Values.Select(v => BFast.ToBytes(v)));
            return r;
        }

        public static S3D ToS3D(this IScene scene, INamedBuffer[] assets)
        {
            var r = new S3D();

            // Compute the node lookup and the list of nodes 
            var nodeLookup = scene.Nodes.ToEnumerable().ToIndexedSet();
            var nodes = nodeLookup.ToOrderedList();

            // Compute the geomery lookup and the list of geometries 
            var geometryLookup = scene.GeometryLookup();
            var geometries = geometryLookup.ToOrderedList();

            // Compute the list of serializable nodes
            r.Nodes = nodes.Select(n =>
               CreateSerializableNode(
                    n.Geometry == null ? -1 : geometryLookup[n.Geometry],
                    n.Parent == null ? -1 : nodeLookup[n.Parent],
                    n.Transform)).ToArray();

            // Get the geometry bytes and write them all to file.
            r.Geometries = geometries.Select((g, i) => g.ToG3D()).ToArray();

            // surfaces.array - An array serializable surfaces
            r.Surfaces = scene.Surfaces.Select(ToSerializableSurface).ToArray();

            // Create a new string table
            var localStrings = new IndexedSet<string>();

            // Get all properties 
            var allSceneProps = scene.Properties.AllProperties;
            var allProps = new List<SerializableProperty>();
            foreach (var k in allSceneProps.Keys.ToEnumerable())
            {
                var localProps = allSceneProps[k].ToSerializableProperties(localStrings);
                allProps.AddRange(localProps);
            }
            r.Properties = allProps.ToArray();

            // TODO: write the props
            r.StringData = localStrings.ToOrderedArray();

            r.Assets = assets;
            return r;
        }

        public static INamedBuffer ToNamedBuffer(this IG3D g, int i)
            => g.ToBytes().ToNamedBuffer(i.ToString());

        public static void WriteToFolder(this S3D scene, string folder)
        {
            // Write the nodes 
            Util.WriteFixedLayoutClassList(Path.Combine(folder, S3D.SectionNameNodesArray), scene.Nodes);

            // Get the geometry bytes and write them all to file.            
            scene.Geometries.Select(ToNamedBuffer).ToBFastFile(Path.Combine(folder, S3D.SectionNameGeometriesBfast));

            // surfaces.array - An array serializable surfaces
            Util.WriteFixedLayoutClassList(Path.Combine(folder, S3D.SectionNameSurfacesArray), scene.Surfaces);

            // WRite the properties 

            // Write the string data 
            var stringData = string.Join("\0", scene.StringData);
            File.WriteAllText(Path.Combine(folder, S3D.SectionNameStringData), stringData);

            // Wri{{3.231te the assets 
            scene.Assets.ToBFastFile(Path.Combine(folder, S3D.SectionNameAssetsBfast));
        }

        public static IScene ReadFromFolder(string folder)
        {
            var metaJson = File.ReadAllText(Path.Combine(folder, S3D.SectionNameMetaJson));

            var serializedNodes = Util.ReadFixedLayoutClassList<SerializableNode>(Path.Combine(folder, S3D.SectionNameNodesArray));

            var g3dBFastFile = Path.Combine(folder, S3D.SectionNameGeometriesBfast);
            var g3dBFast = BFast.Read(g3dBFastFile);

            // TODO: extract the geometries (with names) from the g3DBFast 

            // surfaces.array - An array serializable surfaces
            var surfaceFileName = Path.Combine(folder, S3D.SectionNameSurfacesArray);
            var serializableSurfaces = Util.ReadFixedLayoutClassList<SerializableSurface>(surfaceFileName);

            // Create a new string table
            var localStrings = new IndexedSet<string>();

            // Read all properties 
            var propsBFast = Util.ReadFixedLayoutClassList<SerializableProperty>(Path.Combine(folder, S3D.SectionNamePropertiesBFast));

            // TODO: get the dictionary out of the propsBFast. Note that each propertySet is going to be an array of props, so I will have to cast it.

            // Read the string data 
            var rawStringData = File.ReadAllText(Path.Combine(folder, S3D.SectionNameStringData));
            var stringTable = rawStringData.SplitAtNull();

            // Read the assets 
            var assetsFilePath = Path.Combine(folder, S3D.SectionNameAssetsBfast);
            var assets = BFast.Read(assetsFilePath);

            // 
            throw new NotImplementedException();
        }

        public static S3D ToS3D(this IScene scene)
        {
            var meta = "{ \"version\": \"1.0.0\", \"filetype\", \"s3d\" }";
            var nodes = scene.AllNodes().ToArray();
            var geometries = scene.AllDistinctGeometries().ToArray();
            var surfaces = new ISurfaceRelation[0];
            var assets = new INamedBuffer[0];
            var props = scene.Properties;
            return new S3D {
                Meta = meta,
                Root = scene.Root, nodes, geometries, surfaces, assets, props };
        }
    }
}

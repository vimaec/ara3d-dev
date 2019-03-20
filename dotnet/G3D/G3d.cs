/*
    G3D Geometry Format Library
    Copyright 2018, Ara 3D, Inc.
    Usage licensed under terms of MIT License

    The G3D format is a simple, generic, and efficient representation of geometry data. 
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ara3D
{ 
    /// <summary>
    /// The interface of an IG3D structure which is just a collection of Attributes.
    /// Strictly speaking an IG3D could be just a collection of Attributes,
    /// but there are a couple of basic attribute accessors that we require to be present
    /// to simplify coding.
    /// </summary>
    public interface IG3D
    {
        /// <summary>
        /// All of the attributes present in the IG3D container. 
        /// </summary>
        IEnumerable<IAttribute> Attributes { get; }

        /// <summary>
        ///  All geometries must have a vertex buffer. It may be single or double precision floats, arranged in triples.
        /// </summary>
        IAttribute VertexAttribute { get; }

        /// <summary>
        /// The index attribute is optional: if missing then we assume that all vertices are detached. 
        /// </summary>
        IAttribute IndexAttribute { get; }

        /// <summary>
        /// The face-size attribute is optional: if missing we assume a triangular mesh, if present it is either per-object or per-face.
        /// If per-object then it is a fixed size quad-mesh, tri-mesh, line-set, or a point cloud.
        /// If per-face then we are dealing with a poly-mesh (different sized faces). In that case we
        /// also expect a FaceIndexAttribute.
        /// </summary>
        IAttribute FaceSizeAttribute { get; }

        /// <summary>
        /// The face-index attribute is optional: it should only be present when dealing with a poly-mesh which
        /// indicates that the face-size attribute is present and per-face. 
        /// </summary>
        IAttribute FaceIndexAttribute { get; }

        /// <summary>
        /// The material id attribute is optional: it returns a material id for each face.
        /// </summary>
        IAttribute MaterialIdAttribute { get; }
    }

    /// <summary>
    /// A wrapper around a memory buffer, or BFast struct containing attributes. 
    /// </summary>
    public class G3D : IG3D
    {
        public static string DefaultHeader = new {
            file = "g3d",
            g3dversion = new G3DVersion(),
        }.ToJson();

        public IEnumerable<IAttribute> Attributes { get; }

        public IAttribute VertexAttribute { get; }
        public IAttribute IndexAttribute { get; }
        public IAttribute FaceSizeAttribute { get; }
        public IAttribute FaceIndexAttribute { get; }
        public IAttribute MaterialIdAttribute { get; }

        public string Header { get; }

        /// <summary>
        /// Optional BFast buffer
        /// </summary>
        public readonly BFast Buffer;

        public G3D(IEnumerable<IAttribute> attributes, BFast buffer = null, string header = null)
        {
            Buffer = buffer;
            Header = header;
            Attributes = attributes.WhereNotNull();
            
            VertexAttribute = this.FindAttribute(AttributeType.attr_vertex);
            IndexAttribute = this.FindAttribute(AttributeType.attr_index, false);
            FaceSizeAttribute = this.FindAttribute(AttributeType.attr_facesize, false);
            FaceIndexAttribute = this.FindAttribute(AttributeType.attr_faceindex, false);
            MaterialIdAttribute = this.FindAttribute(AttributeType.attr_materialid, false);

            // Check that everything is kosher
            // TODO: this is a long process, only do it in specific circumstances.
            //this.Validate();
        }

        public static G3D Create(BFast bfast)
        {
            if (bfast.Buffers.Count < 2)
                throw new Exception("Expected at least two data buffers in file: header, and attribute descriptor array");                

            var header = bfast.Buffers[0].ToBytes().ToUtf8();
            var descriptors = bfast.Buffers[1].ToStructs<AttributeDescriptor>();
            var buffers = bfast.Buffers.Skip(2).ToArray();
            if (descriptors.Length != buffers.Length)
                throw new Exception("Incorrect number of descriptors");

            return new G3D(buffers.Zip(descriptors, G3DExtensions.ToAttribute), bfast, header);
        }

        // TODO: all of these copies make me die a little bit inside
        public static G3D Create(IBytes bytes)
            => Create(bytes.ToBytes());    

        public static G3D Create(byte[] bytes)
            => Create(new Memory<byte>(bytes));

        public static G3D Create(Memory<byte> memory)
            => Create(new BFast(memory));

        public static G3D ReadFile(string filePath)
            => Create(File.ReadAllBytes(filePath));

        public static G3D Create(IEnumerable<IAttribute> attributes)
            => new G3D(attributes);

        public static G3D Create(params IAttribute[] attributes)
            => new G3D(attributes);
    }
}

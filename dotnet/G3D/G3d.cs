/*
    G3D Geometry Format Library
    Copyright 2018, Ara 3D, Inc.
    Usage licensed under terms of MIT License

    The G3D format is a simple, generic, and efficient representation of geometry data. 
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ara3D
{ 
    /// <summary>
    /// The interface of an IG3D structure which is just a collection of Attributes.  
    /// </summary>
    public interface IG3D
    {
        IEnumerable<IAttribute> Attributes { get; }

        // All geometries must have a vertex buffer 
        IAttribute VertexAttribute { get; }

        // The index attribute is optional: if missing then we assume that all indices are detached 
        IAttribute IndexAttribute { get; }

        // The face-size attribute is optional: if missing we assume a triangular mesh, if present it is either per-object or per-face. 
        IAttribute FaceSizeAttribute { get; }

        // The face-index attribute is optional: it should only be present when the face-size attribute is present and per-face. If missing in that case, it is computed from the FaceSizeAttribute.
        IAttribute FaceIndexAttribute { get; }
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
            if (VertexAttribute.Descriptor.Association != Association.assoc_vertex)
                throw new Exception("Vertex buffer is not associated with vertex: " + VertexAttribute.Descriptor);

            if (VertexAttribute.Descriptor.DataArity != 3)
                throw new Exception("Vertices should have an arity of 3");

            IndexAttribute = this.FindAttribute(AttributeType.attr_index, false);
            if (IndexAttribute != null)
            {
                if (IndexAttribute.Descriptor.Association != Association.assoc_corner)
                    throw new Exception("Index buffer is not associated with index: " + IndexAttribute.Descriptor);
                if (IndexAttribute.Descriptor.DataArity != 1)
                    throw new Exception("Index buffer should have an arity of 1");
            }

            FaceSizeAttribute = this.FindAttribute(AttributeType.attr_facesize, false);
            if (FaceSizeAttribute != null)
            {
                if (FaceSizeAttribute.Descriptor.Association != Association.assoc_face)
                    throw new Exception("Face size attribute is not associated with faces: " + FaceSizeAttribute.Descriptor);
                if (FaceSizeAttribute.Descriptor.DataArity != 1)
                    throw new Exception("Expected an arity of 1");
                if (FaceSizeAttribute.Descriptor.DataType != DataType.dt_int32)
                    throw new Exception("Face size attribute is expected to be a 32 bit integer");
            }

            FaceIndexAttribute = this.FindAttribute(AttributeType.attr_faceindex, false);
            if (FaceIndexAttribute != null)
            {
                if (FaceIndexAttribute.Descriptor.Association != Association.assoc_face)
                    throw new Exception("Face index attribute is not associated with faces: " + FaceIndexAttribute.Descriptor);
                if (FaceIndexAttribute.Descriptor.DataArity != 1)
                    throw new Exception("Expected an arity of 1");
                if (FaceIndexAttribute.Descriptor.DataType != DataType.dt_int32)
                    throw new Exception("Face size attribute is expected to be a 32 bit integer");
            }
            
            // NOW: compute the number of faces, and make sure that all of the attributes make sense. This is going to be a lot of work!! 
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

        public static G3D Create(byte[] bytes)
            => Create(new Memory<byte>(bytes));

        public static G3D Create(Memory<byte> memory)
            => Create(new BFast(memory));
    }
}

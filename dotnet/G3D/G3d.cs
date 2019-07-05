/*
    G3D Geometry Format Library
    Copyright 2018, Ara 3D, Inc.
    Usage licensed under terms of MIT License

    The G3D format is a simple, generic, and efficient representation of geometry data. 
    It is based on the BFAST container form.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ara3D
{ 
    /// <summary>
    /// The interface of an IG3D structure which is just a collection of Attributes.
    /// Strictly speaking an IG3D could be nothing else but a collection of Attributes,
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
        public static string DefaultHeader = new Func<string>(() =>
        {
            // A tailored string to avoid a JSON serialization dependency.
            var file = "g3d";
            var g3dversion = new G3DVersion();
            return (
$@"{{
  ""file"": ""{file}"",
  ""g3dversion"": {{
    ""Major"": ""{g3dversion.Major}"",
    ""Minor"": ""{g3dversion.Minor}"",
    ""Revision"": ""{g3dversion.Revision}"",
    ""Date"": ""{g3dversion.Date}""
  }}
}}"
            );
        })();

        public IEnumerable<IAttribute> Attributes { get; }

        public IAttribute VertexAttribute => m_vertexAttribute;
        public IAttribute IndexAttribute => m_indexAttribute;
        public IAttribute FaceSizeAttribute => m_faceSizeAttribute;
        public IAttribute FaceIndexAttribute => m_faceIndexAttribute;
        public IAttribute MaterialIdAttribute => m_materialIdAttribute;

        public string Header { get; }

        private readonly IAttribute m_vertexAttribute;
        private readonly IAttribute m_indexAttribute;
        private readonly IAttribute m_faceSizeAttribute;
        private readonly IAttribute m_faceIndexAttribute;
        private readonly IAttribute m_materialIdAttribute;

        public G3D(params IAttribute[] attributes)
            : this(attributes, null)
        { }

        public G3D(IEnumerable<IAttribute> attributes, string header = null, bool validate = false)
        {
            Header = header;
            Attributes = attributes;

            foreach (var attr in attributes)
            {
                _AssignIfPossible(attr, ref m_vertexAttribute, AttributeType.attr_vertex);
                _AssignIfPossible(attr, ref m_indexAttribute, AttributeType.attr_index);
                _AssignIfPossible(attr, ref m_faceSizeAttribute, AttributeType.attr_facesize);
                _AssignIfPossible(attr, ref m_faceIndexAttribute, AttributeType.attr_faceindex);
                _AssignIfPossible(attr, ref m_materialIdAttribute, AttributeType.attr_materialid);
            }

            if (m_vertexAttribute == null)
                throw new Exception("Vertex attribute is not present");

            if (validate)
                this.Validate();
        }

        private static void _AssignIfPossible(IAttribute attr, ref IAttribute target, AttributeType at)
        {
            if (attr == null) return;
            if (attr.Descriptor.AttributeType == at)
            {
                if (target != null) throw new Exception("Attribute is already assigned");
                target = attr;
            }
        }

        public static G3D Create(IEnumerable<IBuffer> buffers)
            => Create(buffers.ToList());

        public static G3D Create(IList<IBuffer> buffers)
        {
            if (buffers.Count < 2)
                throw new Exception("Expected at least two data buffers in file: header, and attribute descriptor array");                

            var header = buffers[0].Bytes.ToArray().ToUtf8();
            var descriptors = buffers[1].Bytes.ToStructs<AttributeDescriptor>().ToArray();
            buffers = buffers.Skip(2).ToList();
            if (descriptors.Length != buffers.Count)
                throw new Exception("Incorrect number of descriptors");

            // TODO: this guy is going to be hard to process 
            // I have raw bytes, and I have to cast it to the correct type.
            // That correct type depends on the type flag stored in the descriptor 
            return new G3D(buffers.Zip(descriptors, G3DExtensions.ToAttribute), header);
        }

        // TODO: all of these copies make me die a little bit inside
        public static G3D Create(byte[] bytes)
            => Create(new Memory<byte>(bytes));

        public static G3D Create(Memory<byte> bytes)
            => Create(BFast.ToBFastRawBuffers(bytes));

        public static G3D ReadFile(string filePath)
            => Create(File.ReadAllBytes(filePath));

        public static G3D Create(IEnumerable<IAttribute> attributes)
            => new G3D(attributes);

        public static G3D Create(params IAttribute[] attributes)
            => new G3D(attributes);
    }
}

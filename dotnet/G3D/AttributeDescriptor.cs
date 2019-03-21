using System;
using System.Runtime.InteropServices;

namespace Ara3D
{
    [StructLayout(LayoutKind.Explicit, Pack = 0, Size = 32)]
    public struct AttributeDescriptor
    {
        [FieldOffset(0)] public int _association;           // Indicates the part of the geometry that this attribute is associated with 
        [FieldOffset(4)] public int _attribute_type;        // n integer values 
        [FieldOffset(8)] public int _attribute_type_index;  // each attribute type should have it's own index ( you can have uv0, uv1, etc. )
        [FieldOffset(12)] public int _data_arity;           // how many values associated with each element (e.g. UVs might be 2, geometry might be 3, quaternions 4, matrices 9 or 16)
        [FieldOffset(16)] public int _data_type;            // the type of individual values (e.g. int32, float64)
        [FieldOffset(20)] public int _pad0;                 // ignored, used to bring the alignment up to a power of two.
        [FieldOffset(24)] public int _pad1;                 // ignored, used to bring the alignment up to a power of two. 
        [FieldOffset(28)] public int _pad2;                 // ignored, used to bring the alignment up to a power of two.

        public int ItemSize => DataTypeSize(_data_type) * _data_arity;

        public Association Association => (Association) _association;
        public AttributeType AttributeType => (AttributeType)_attribute_type;
        public int AttributeTypeIndex => _attribute_type_index;
        public int DataArity => _data_arity;
        public DataType DataType => (DataType)_data_type;
        public Type TypeOfData => DataType.ToType();

        /// <summary>
        /// Generates a URN representation of the attribute descriptor
        /// </summary>
        public override string ToString()
            => $"g3d:{AttributeTypeToString(_attribute_type)}:{AssociationToString(_association)}:{_attribute_type_index}:{DataTypeToString(_data_type)}:{_data_arity}";
        
        /// <summary>
        /// Parses a URN representation of the attribute descriptor to generate an actual attribute descriptor 
        /// </summary>
        public static AttributeDescriptor Parse(string urn)
        {
            var vals = urn.Split(':');
            if (vals.Length != 6) throw new Exception("Expected 6 parts to the attribute descriptor URN");
            if (vals[0] != "g3d") throw new Exception("First part of URN must be g3d");
            return new AttributeDescriptor
            {
                _attribute_type = ParseAttributeType(vals[1]),
                _association = ParseAssociation(vals[2]),
                _attribute_type_index = int.Parse(vals[3]),
                _data_type = ParseDataType(vals[4]),
                _data_arity = int.Parse(vals[5]),
            };
        }

        public void Validate()
        {
            var urn = ToString();
            var tmp = Parse(urn);
            if (!Equals(tmp))
                throw new Exception("Invalid attribute descriptor (or internal error in the parsing/string conversion");
        }

        public bool Equals(AttributeDescriptor other)
        {
            return _data_type == other._data_type
                   && _association == other._association
                   && _attribute_type == other._attribute_type
                   && _attribute_type_index == other._attribute_type_index
                   && _data_type == other._data_type
                   && _data_arity == other._data_arity;
        }

        public static int DataTypeSize(int n) => DataTypeSize((DataType) n);

        public static int DataTypeSize(DataType dt)
        {
            switch (dt)
            {
                case DataType.dt_int8: return 1;
                case DataType.dt_int16: return 2;
                case DataType.dt_int32: return 4;
                case DataType.dt_int64: return 8;
                case DataType.dt_float32: return 4;
                case DataType.dt_float64: return 8;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dt), dt, null);
            }
        }

        public static string AssociationToString(int n) 
            => Enum.GetName(typeof(Association), n)?.Substring("assoc_".Length);

        public static int ParseAssociation(string s) 
            => (int)Enum.Parse(typeof(Association), "assoc_" + s);

        public static string AttributeTypeToString(int n) 
            => Enum.GetName(typeof(AttributeType), n)?.Substring("attr_".Length);

        public static int ParseAttributeType(string s) 
            => (int)Enum.Parse(typeof(AttributeType), "attr_" + s);

        public static string DataTypeToString(int n) 
            => Enum.GetName(typeof(DataType), n)?.Substring("dt_".Length);

        public static int ParseDataType(string s) 
            => (int)Enum.Parse(typeof(DataType), "dt_" + s);
    }
}
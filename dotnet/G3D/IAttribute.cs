using System.Diagnostics;

namespace Ara3D
{
    public interface IAttribute
    {
        AttributeDescriptor Descriptor { get; }

        // This is the number of data items in the attribute if there were 4 vector3 this would be 12. 
        int DataCount { get; } 

        // TODO: should be Memory<byte> ??
        byte[] Bytes { get; }

        IArray<int> ToInts();
        IArray<byte> ToBytes();
        IArray<short> ToShorts();
        IArray<long> ToLongs();
        IArray<float> ToFloats();
        IArray<double> ToDoubles();
        IArray<Vector2> ToVector2s();
        IArray<Vector3> ToVector3s();
        IArray<Vector4> ToVector4s();
        IArray<Matrix4x4> ToMatrices();
        IArray<DVector2> ToDVector2s();
        IArray<DVector3> ToDVector3s();
        IArray<DVector4> ToDVector4s();
    }

    public class AttributeArray<T> : IAttribute where T: struct
    {
        private IArray<T> _data { get; }

        public AttributeDescriptor Descriptor { get; }
        public int DataCount { get; }
        public byte[] Bytes => Util.ArrayToBytes(_data.ToArray());

        public IArray<int> ToInts() => _data.ToInts();
        public IArray<byte> ToBytes() => _data.ToBytes();
        public IArray<short> ToShorts() => _data.ToShorts();
        public IArray<long> ToLongs() => _data.ToLongs();
        public IArray<float> ToFloats() => _data.ToFloats();
        public IArray<double> ToDoubles() => _data.ToDoubles();
        public IArray<Vector2> ToVector2s() => _data.ToVector2s();
        public IArray<Vector3> ToVector3s() => _data.ToVector3s();
        public IArray<Vector4> ToVector4s() => _data.ToVector4s();
        public IArray<Matrix4x4> ToMatrices() => _data.ToMatrices();
        public IArray<DVector2> ToDVector2s() => _data.ToDVector2s();
        public IArray<DVector3> ToDVector3s() => _data.ToDVector3s();
        public IArray<DVector4> ToDVector4s() => _data.ToDVector4s();

        public AttributeArray(IArray<T> data, AttributeDescriptor desc)
        {
            _data = data;
            Descriptor = desc;
            var typeArity = G3DExtensions.TypeArity<T>();
            DataCount = _data.Count * typeArity;
        }
    }
}
namespace Ara3D
{
    public interface IAttribute
    {
        AttributeDescriptor Descriptor { get; }
        int Count { get; }

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
        public IArray<T> Data;
        public AttributeDescriptor Descriptor { get; }
        public int Count => Data.Count;
        public byte[] Bytes => Util.ArrayToBytes(Data.ToArray());

        public IArray<int> ToInts() => Data.ToInts();
        public IArray<byte> ToBytes() => Data.ToBytes();
        public IArray<short> ToShorts() => Data.ToShorts();
        public IArray<long> ToLongs() => Data.ToLongs();
        public IArray<float> ToFloats() => Data.ToFloats();
        public IArray<double> ToDoubles() => Data.ToDoubles();
        public IArray<Vector2> ToVector2s() => Data.ToVector2s();
        public IArray<Vector3> ToVector3s() => Data.ToVector3s();
        public IArray<Vector4> ToVector4s() => Data.ToVector4s();
        public IArray<Matrix4x4> ToMatrices() => Data.ToMatrices();
        public IArray<DVector2> ToDVector2s() => Data.ToDVector2s();
        public IArray<DVector3> ToDVector3s() => Data.ToDVector3s();
        public IArray<DVector4> ToDVector4s() => Data.ToDVector4s();

        public AttributeArray(IArray<T> data, AttributeDescriptor desc)
        {
            Data = data;
            Descriptor = desc;
        }
    }
}
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Ara3D
{
    public interface IAttribute
    {
        AttributeDescriptor Descriptor { get; }
        int Count { get; }
        IBytes Bytes { get; }

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

    public class AttributeArray<T> : IAttribute
    {
        public IArray<T> Data;
        public AttributeDescriptor Descriptor { get; }
        public int Count => Data.Count;
        public IBytes Bytes => Data.ToArray().Pin();

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

    public unsafe class AttributeBytes : IAttribute
    {
        public IBytes Bytes { get; }
        public AttributeDescriptor Descriptor { get; }
        public int Count { get; }

        public AttributeBytes(IBytes bytes, AttributeDescriptor descriptor)
        {
            Bytes = bytes;
            Descriptor = descriptor;

            // Should never happen, but just in case
            if (Bytes.ByteCount % Descriptor.ItemSize != 0)
                throw new Exception("Number of items does not divide by item size properly");

            Count = Bytes.ByteCount / Descriptor.ItemSize;
        }


        private int ResizeCount<T>()
            => (Count * Descriptor.ItemSize) / Marshal.SizeOf(typeof(T));

        public IArray<int> ToInts() => ResizeCount<int>().Select(i => ((int*)Bytes.Ptr)[i]);
        public IArray<byte> ToBytes() => ResizeCount<byte>().Select(i => ((byte*)Bytes.Ptr)[i]);
        public IArray<short> ToShorts() => ResizeCount<short>().Select(i => ((short*)Bytes.Ptr)[i]);
        public IArray<long> ToLongs() => ResizeCount<long>().Select(i => ((long*)Bytes.Ptr)[i]);
        public IArray<float> ToFloats() => ResizeCount<float>().Select(i => ((float*)Bytes.Ptr)[i]);
        public IArray<double> ToDoubles() => ResizeCount<double>().Select(i => ((double*)Bytes.Ptr)[i]);
        public IArray<Vector2> ToVector2s() => ResizeCount<Vector2>().Select(i => ((Vector2*)Bytes.Ptr)[i]);
        public IArray<Vector3> ToVector3s() => ResizeCount<Vector3>().Select(i => ((Vector3*)Bytes.Ptr)[i]);
        public IArray<Vector4> ToVector4s() => ResizeCount<Vector4>().Select(i => ((Vector4*)Bytes.Ptr)[i]);
        public IArray<Matrix4x4> ToMatrices() => ResizeCount<Matrix4x4>().Select(i => ((Matrix4x4*)Bytes.Ptr)[i]);
        public IArray<DVector2> ToDVector2s() => ResizeCount<DVector2>().Select(i => ((DVector2*)Bytes.Ptr)[i]);
        public IArray<DVector3> ToDVector3s() => ResizeCount<DVector3>().Select(i => ((DVector3*)Bytes.Ptr)[i]);
        public IArray<DVector4> ToDVector4s() => ResizeCount<DVector4>().Select(i => ((DVector4*)Bytes.Ptr)[i]);
    }

}
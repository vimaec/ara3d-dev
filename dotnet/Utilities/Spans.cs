
using System;
using System.Numerics;

namespace Ara3D
{
    public unsafe class Int32Span : BaseSpan, IArray< Int32 >
    {
        Int32Span(IByteSpan bytes) 
			: base(bytes, bytes.ByteCount / sizeof(Int32))
		{ }

        public Int32 this[int n] => ((Int32*)Bytes.Ptr)[n];
    }

    public unsafe class Int64Span : BaseSpan, IArray< Int64 >
    {
        Int64Span(IByteSpan bytes) 
			: base(bytes, bytes.ByteCount / sizeof(Int64))
		{ }

        public Int64 this[int n] => ((Int64*)Bytes.Ptr)[n];
    }

    public unsafe class SingleSpan : BaseSpan, IArray< Single >
    {
        SingleSpan(IByteSpan bytes) 
			: base(bytes, bytes.ByteCount / sizeof(Single))
		{ }

        public Single this[int n] => ((Single*)Bytes.Ptr)[n];
    }

    public unsafe class DoubleSpan : BaseSpan, IArray< Double >
    {
        DoubleSpan(IByteSpan bytes) 
			: base(bytes, bytes.ByteCount / sizeof(Double))
		{ }

        public Double this[int n] => ((Double*)Bytes.Ptr)[n];
    }

    public unsafe class Vector2Span : BaseSpan, IArray< Vector2 >
    {
        Vector2Span(IByteSpan bytes) 
			: base(bytes, bytes.ByteCount / sizeof(Vector2))
		{ }

        public Vector2 this[int n] => ((Vector2*)Bytes.Ptr)[n];
    }

    public unsafe class Vector3Span : BaseSpan, IArray< Vector3 >
    {
        Vector3Span(IByteSpan bytes) 
			: base(bytes, bytes.ByteCount / sizeof(Vector3))
		{ }

        public Vector3 this[int n] => ((Vector3*)Bytes.Ptr)[n];
    }

    public unsafe class Vector4Span : BaseSpan, IArray< Vector4 >
    {
        Vector4Span(IByteSpan bytes) 
			: base(bytes, bytes.ByteCount / sizeof(Vector4))
		{ }

        public Vector4 this[int n] => ((Vector4*)Bytes.Ptr)[n];
    }

}
using System;
using System.Numerics;

namespace Ara3D
{
    /// <summary>
    /// TODO: I would have liked to place this in the Math library, but it currently has no dependencies on LinqArray (or anything else).
    /// I am planning to move LinqArray functionality into the math library. It is going to enhance the library significantly I think. 
    /// </summary>
    public static class ArrayCast
    {
        // Specializations 
        public static IArray<int> ToInts(this IArray<int> xs) => xs;
        public static IArray<int> ToInts(this IArray<byte> xs) => xs.Select(x => (int) x);
        public static IArray<int> ToInts(this IArray<short> xs) => xs.Select(x => (int)x);
        public static IArray<byte> ToBytes(this IArray<byte> xs) => xs;
        public static IArray<short> ToShorts(this IArray<short> xs) => xs;
        public static IArray<long> ToLongs(this IArray<long> xs) => xs;
        public static IArray<float> ToFloats(this IArray<float> xs) => xs;
        public static IArray<float> ToFloats(this IArray<Vector2> xs) => xs.SelectMany(x => Tuple.Create(x.X, x.Y));
        public static IArray<float> ToFloats(this IArray<Vector3> xs) => xs.SelectMany(x => Tuple.Create(x.X, x.Y, x.Z));
        public static IArray<float> ToFloats(this IArray<Vector4> xs) => xs.SelectMany(x => Tuple.Create(x.X, x.Y, x.Z, x.W));
        public static IArray<double> ToDoubles(this IArray<double> xs) => xs;
        public static IArray<double> ToDoubles(this IArray<DVector2> xs) => xs.SelectMany(x => Tuple.Create(x.X, x.Y));
        public static IArray<double> ToDoubles(this IArray<DVector3> xs) => xs.SelectMany(x => Tuple.Create(x.X, x.Y, x.Z));
        public static IArray<double> ToDoubles(this IArray<DVector4> xs) => xs.SelectMany(x => Tuple.Create(x.X, x.Y, x.Z, x.W));
        public static IArray<Vector2> ToVector2s(this IArray<Vector2> xs) => xs;
        public static IArray<Vector3> ToVector3s(this IArray<Vector3> xs) => xs;
        public static IArray<Vector4> ToVector4s(this IArray<Vector4> xs) => xs;
        public static IArray<Matrix4x4> ToMatrices(this IArray<Matrix4x4> xs) => xs;
        public static IArray<DVector2> ToDVector2s(this IArray<DVector2> xs) => xs;
        public static IArray<DVector3> ToDVector3s(this IArray<DVector3> xs) => xs;
        public static IArray<DVector4> ToDVector4s(this IArray<DVector4> xs) => xs;
        public static IArray<DVector2> ToDVector2s(this IArray<Vector2> xs) => xs.Select(x => new DVector2(x.X, x.Y));
        public static IArray<DVector3> ToDVector3s(this IArray<Vector3> xs) => xs.Select(x => new DVector3(x.X, x.Y, x.Z));
        public static IArray<DVector4> ToDVector4s(this IArray<Vector4> xs) => xs.Select(x => new DVector4(x.X, x.Y, x.Z, x.W));

        // Generic fallbacks
        public static IArray<int> ToInts<T>(this IArray<T> xs) => throw new InvalidCastException();
        public static IArray<byte> ToBytes<T>(this IArray<T> xs) => throw new InvalidCastException();
        public static IArray<short> ToShorts<T>(this IArray<T> xs) => throw new InvalidCastException();
        public static IArray<long> ToLongs<T>(this IArray<T> xs) => xs.ToInts().Select(x => (long) x);
        public static IArray<float> ToFloats<T>(this IArray<T> xs) => throw new InvalidCastException();
        public static IArray<double> ToDoubles<T>(this IArray<T> xs) => xs.ToFloats().Select(x => (double)x);
        public static IArray<Vector2> ToVector2s<T>(this IArray<T> xs) => xs.ToFloats().SelectPairs((x, y) => new Vector2(x, y));
        public static IArray<Vector3> ToVector3s<T>(this IArray<T> xs) => xs.ToFloats().SelectTriplets((x, y, z) => new Vector3(x, y, z));
        public static IArray<Vector4> ToVector4s<T>(this IArray<T> xs) => xs.ToFloats().SelectQuartets((x, y, z, w) => new Vector4(x, y, z, w));
        public static IArray<Matrix4x4> ToMatrices<T>(this IArray<T> xs) => throw new InvalidCastException();
        public static IArray<DVector2> ToDVector2s<T>(this IArray<T> xs) => xs.ToDoubles().SelectPairs((x, y) => new DVector2(x, y));
        public static IArray<DVector3> ToDVector3s<T>(this IArray<T> xs) => xs.ToDoubles().SelectTriplets((x, y, z) => new DVector3(x, y, z));
        public static IArray<DVector4> ToDVector4s<T>(this IArray<T> xs) => xs.ToDoubles().SelectQuartets((x, y, z, w) => new DVector4(x, y, z, w));
    }
}
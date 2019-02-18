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
        public static IArray<int> ToInts<T>(this IArray<T> xs) => 
            xs is IArray<int> r 
                ? r 
                : xs is IArray<short> r1 
                    ? r1.Select(x => (int) x) 
                    : xs is IArray<byte> r2 
                        ? r2.Select(x => (int)x)
                        : throw new NotImplementedException();

        public static IArray<byte> ToBytes<T>(this IArray<T> xs) =>
            xs is IArray<byte> r
                ? r
                : throw new NotImplementedException();

        public static IArray<short> ToShorts<T>(this IArray<T> xs) =>
            xs is IArray<short> r
                ? r
                : throw new NotImplementedException();

        public static IArray<long> ToLongs<T>(this IArray<T> xs) =>
            xs is IArray<long> r
                ? r
                : xs.ToInts().Select(x => (long)x);

        public static IArray<float> ToFloats<T>(this IArray<T> xs) =>
            xs is IArray<float> r
                ? r
                : xs is IArray<Vector2> r1
                    ? r1.SelectMany(x => Tuple.Create(x.X, x.Y))
                    : xs is IArray<Vector3> r2
                        ? r2.SelectMany(x => Tuple.Create(x.X, x.Y, x.Z))
                        : xs is IArray<Vector4> r3
                            ? r3.SelectMany(x => Tuple.Create(x.X, x.Y, x.Z, x.W))
                            : xs.ToInts().Select(x => (float) x);

        public static IArray<double> ToDoubles<T>(this IArray<T> xs) =>
            xs is IArray<double> r
                ? r
                : xs is IArray<DVector2> r1
                    ? r1.SelectMany(x => Tuple.Create(x.X, x.Y))
                    : xs is IArray<DVector3> r2
                        ? r2.SelectMany(x => Tuple.Create(x.X, x.Y, x.Z))
                        : xs is IArray<DVector4> r3
                            ? r3.SelectMany(x => Tuple.Create(x.X, x.Y, x.Z, x.W))
                            : xs.ToFloats().Select(x => (double) x);

        public static IArray<Vector2> ToVector2s<T>(this IArray<T> xs) => 
            xs is IArray<Vector2> r
                ? r
                : xs.ToFloats().SelectPairs((x, y) => new Vector2(x, y));
                    
        public static IArray<Vector3> ToVector3s<T>(this IArray<T> xs) => 
            xs is IArray<Vector3> r
                ? r
                : xs.ToFloats().SelectTriplets((x, y, z) => new Vector3(x, y, z));

        public static IArray<Vector4> ToVector4s<T>(this IArray<T> xs) =>
            xs is IArray<Vector4> r
                ? r
                : xs.ToFloats().SelectQuartets((x, y, z, w) => new Vector4(x, y, z, w));

        public static IArray<Matrix4x4> ToMatrices<T>(this IArray<T> xs) =>
            xs is IArray<Matrix4x4> r
                ? r
                : throw new NotImplementedException();

        public static IArray<DVector2> ToDVector2s<T>(this IArray<T> xs) =>
            xs is IArray<DVector2> r
                ? r
                : xs.ToDoubles().SelectPairs((x, y) => new DVector2(x, y));

        public static IArray<DVector3> ToDVector3s<T>(this IArray<T> xs) =>
            xs is IArray<DVector3> r
                ? r
                : xs.ToDoubles().SelectTriplets((x, y, z) => new DVector3(x, y, z));

        public static IArray<DVector4> ToDVector4s<T>(this IArray<T> xs) =>
            xs is IArray<DVector4> r
                ? r
                : xs.ToDoubles().SelectQuartets((x, y, z, w) => new DVector4(x, y, z, w));
    }
}
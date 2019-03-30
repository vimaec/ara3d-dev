// MIT License 
// Copyright (C) 2018 Ara 3D. Inc
// Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.CompilerServices;

namespace Ara3D
{
    public static class VectorHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector2 ToVector2(this float v) => new Vector2(v);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector2 ToVector2(this Vector3 v) => new Vector2(v.X, v.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector2 ToVector2(this Vector4 v) => new Vector2(v.X, v.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 ToVector3(this float v) => new Vector3(v);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 ToVector3(this Vector2 v) => new Vector3(v.X, v.Y, 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 ToVector3(this Vector4 v) => new Vector3(v.X, v.Y, v.Z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector4 ToVector4(this float v) => new Vector4(v);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector4 ToVector4(this Vector2 v) => new Vector4(v.X, v.Y, 0, 0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector4 ToVector4(this Vector3 v) => new Vector4(v.X, v.Y, v.Z, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Rotate(this Vector3 self, Vector3 axis, float angle)
            => self.Transform(Matrix4x4.CreateFromAxisAngle(axis, angle));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNonZeroAndValid(this Vector3 self)
            => self.LengthSquared().IsNonZeroAndValid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZeroOrInvalid(this Vector3 self)
            => !self.IsNonZeroAndValid();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPerpendicular(this Vector3 v1, Vector3 v2, float tolerance = Constants.Tolerance)
            // If either vector is vector(0,0,0) the vectors are not perpendicular
            => v1 != Vector3.Zero && v2 != Vector3.Zero && v1.Dot(v2).AlmostZero(tolerance);        

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Projection(this Vector3 v1, Vector3 v2)
            => v2 * (v1.Dot(v2) / v2.LengthSquared());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Rejection(this Vector3 v1, Vector3 v2)
            => v1 - v1.Projection(v2);

        // The smaller of the two possible angles between the two vectors is returned, therefore the result will never be greater than 180 degrees or smaller than -180 degrees.
        // If you imagine the from and to vectors as lines on a piece of paper, both originating from the same point, then the /axis/ vector would point up out of the paper.
        // The measured angle between the two vectors would be positive in a clockwise direction and negative in an anti-clockwise direction.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
            => Angle(from, to) * Math.Sign(axis.Dot(from.Cross(to)));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(this Vector3 v1, Vector3 v2, float tolerance = Constants.Tolerance)
        {
            var d = v1.LengthSquared() * v2.LengthSquared().Sqrt();
            if (d < tolerance)
                return 0;
            return (v1.Dot(v2) / d).Clamp(-1F, 1F).Acos();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBackFace(this Vector3 normal, Vector3 lineOfSight)
            => normal.Dot(lineOfSight) < 0;        

        /// <summary>
        /// Creates a new <see cref="Vector3"/> that contains CatmullRom interpolation of the specified vectors.
        /// </summary>
        public static Vector3 CatmullRom(this Vector3 value1, Vector3 value2, Vector3 value3, Vector3 value4, float amount) =>
            new Vector3(
                MathOps.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount),
                MathOps.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount),
                MathOps.CatmullRom(value1.Z, value2.Z, value3.Z, value4.Z, amount));

        /// <summary>
        /// Creates a new <see cref="Vector3"/> that contains hermite spline interpolation.
        /// </summary>
        public static Vector3 Hermite(this Vector3 value1, Vector3 tangent1, Vector3 value2, Vector3 tangent2, float amount) =>
            new Vector3(
                MathOps.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount),
                MathOps.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount),
                MathOps.Hermite(value1.Z, tangent1.Z, value2.Z, tangent2.Z, amount));

        /// <summary>
        /// Creates a new <see cref="Vector3"/> that contains cubic interpolation of the specified vectors.
        /// </summary>
        public static Vector3 SmoothStep(this Vector3 value1, Vector3 value2, float amount) =>
            new Vector3(
                MathOps.SmoothStep(value1.X, value2.X, amount),
                MathOps.SmoothStep(value1.Y, value2.Y, amount),
                MathOps.SmoothStep(value1.Z, value2.Z, amount));

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Line ToLine(this Vector3 v) => new Line(Vector3.Zero, v);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 Along(this Vector3 v, float d) => v.Normal() * d;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 AlongX(this float self) => Vector3.UnitX * self;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 AlongY(this float self) => Vector3.UnitY * self;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3 AlongZ(this float self) => Vector3.UnitX * self;

        /// <summary>
        /// Computes the cross product of two vectors.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Cross(this Vector3 vector1, Vector3 vector2)
            => new Vector3(
                vector1.Y * vector2.Z - vector1.Z * vector2.Y,
                vector1.Z * vector2.X - vector1.X * vector2.Z,
                vector1.X * vector2.Y - vector1.Y * vector2.X);

        /// <summary>
        /// Returns the mixed product
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double MixedProduct(this Vector3 v1, Vector3 v2, Vector3 v3)
            => v1.Cross(v2).Dot(v3);

        /// <summary>
        /// Returns the reflection of a vector off a surface that has the specified normal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Reflect(this Vector3 vector, Vector3 normal)
            => vector - normal * vector.Dot(normal) * 2f;

        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Transform(this Vector3 position, Matrix4x4 matrix)
            => new Vector3(
                position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41,
                position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42,
                position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43);

        /// <summary>
        /// Transforms a vector normal by the given matrix.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 TransformNormal(this Vector3 normal, Matrix4x4 matrix)
            => new Vector3(
                normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31,
                normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32,
                normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33);

        /// <summary>
        /// Transforms a vector by the given Quaternion rotation value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Transform(this Vector3 value, Quaternion rotation)
        {
            var x2 = rotation.X + rotation.X;
            var y2 = rotation.Y + rotation.Y;
            var z2 = rotation.Z + rotation.Z;

            var wx2 = rotation.W * x2;
            var wy2 = rotation.W * y2;
            var wz2 = rotation.W * z2;
            var xx2 = rotation.X * x2;
            var xy2 = rotation.X * y2;
            var xz2 = rotation.X * z2;
            var yy2 = rotation.Y * y2;
            var yz2 = rotation.Y * z2;
            var zz2 = rotation.Z * z2;

            return new Vector3(
                value.X * (1.0f - yy2 - zz2) + value.Y * (xy2 - wz2) + value.Z * (xz2 + wy2),
                value.X * (xy2 + wz2) + value.Y * (1.0f - xx2 - zz2) + value.Z * (yz2 - wx2),
                value.X * (xz2 - wy2) + value.Y * (yz2 + wx2) + value.Z * (1.0f - xx2 - yy2));
        }

        /// <summary>
        /// Returns the reflection of a vector off a surface that has the specified normal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Reflect(Vector2 vector, Vector2 normal)
            => vector - (2 * (vector.Dot(normal) * normal));

        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Transform(this Vector2 position, Matrix4x4 matrix)
            => new Vector2(
                position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41,
                position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42);

        /// <summary>
        /// Transforms a vector normal by the given matrix.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 TransformNormal(Vector2 normal, Matrix4x4 matrix)
            => new Vector2(
                normal.X * matrix.M11 + normal.Y * matrix.M21,
                normal.X * matrix.M12 + normal.Y * matrix.M22);

        /// <summary>
        /// Transforms a vector by the given Quaternion rotation value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Transform(this Vector2 value, Quaternion rotation)
        {
            var x2 = rotation.X + rotation.X;
            var y2 = rotation.Y + rotation.Y;
            var z2 = rotation.Z + rotation.Z;

            var wz2 = rotation.W * z2;
            var xx2 = rotation.X * x2;
            var xy2 = rotation.X * y2;
            var yy2 = rotation.Y * y2;
            var zz2 = rotation.Z * z2;

            return new Vector2(
                value.X * (1.0f - yy2 - zz2) + value.Y * (xy2 - wz2),
                value.X * (xy2 + wz2) + value.Y * (1.0f - xx2 - zz2));
        }
    }
}

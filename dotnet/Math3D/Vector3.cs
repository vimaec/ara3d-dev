// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;
using System.Numerics.Hashing;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ara3D
{
    /// <summary>
    /// A structure encapsulating three single precision floating point values and provides hardware accelerated methods.
    /// </summary>
    public partial struct Vector3 
    {
        /// <summary>
        /// Computes the cross product of two vectors.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
            => new Vector3(
                vector1.Y * vector2.Z - vector1.Z * vector2.Y,
                vector1.Z * vector2.X - vector1.X * vector2.Z,
                vector1.X * vector2.Y - vector1.Y * vector2.X);

        /// <summary>
        /// Returns the cross product between this Vector and another 
        /// </summary>
        public Vector3 Cross(Vector3 other)
            => Cross(this, other);

        /// <summary>
        /// Returns the mixed product
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double MixedProduct(Vector3 v1, Vector3 v2, Vector3 v3)
            => v1.Cross(v2).Dot(v3);       

        /// <summary>
        /// Returns the reflection of a vector off a surface that has the specified normal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Reflect(Vector3 vector, Vector3 normal)
            => vector - normal * Dot(vector, normal) * 2f;

        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Transform(Vector3 position, Matrix4x4 matrix)
            => new Vector3(
                position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41,
                position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42,
                position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43);

        /// <summary>
        /// Transforms a vector normal by the given matrix.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 TransformNormal(Vector3 normal, Matrix4x4 matrix)
            => new Vector3(
                normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31,
                normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32,
                normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33);        

        /// <summary>
        /// Transforms a vector by the given Quaternion rotation value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Transform(Vector3 value, Quaternion rotation)
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
    }
}

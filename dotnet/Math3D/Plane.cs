// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ara3D
{
    /// <summary>
    /// A structure encapsulating a 3D Plane
    /// </summary>
    public partial struct Plane 
    {
        /// <summary>
        /// Creates a Plane that contains the three given points.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane CreateFromVertices(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            if (Vector.IsHardwareAccelerated)
            {
                var a = point2 - point1;
                var b = point3 - point1;

                // N = Cross(a, b)
                var n = Vector3.Cross(a, b);
                var normal = Vector3.Normalize(n);

                // D = - Dot(N, point1)
                var d = -Vector3.Dot(normal, point1);

                return new Plane(normal, d);
            }
            else
            {
                var ax = point2.X - point1.X;
                var ay = point2.Y - point1.Y;
                var az = point2.Z - point1.Z;

                var bx = point3.X - point1.X;
                var by = point3.Y - point1.Y;
                var bz = point3.Z - point1.Z;

                // N=Cross(a,b)
                var nx = ay * bz - az * by;
                var ny = az * bx - ax * bz;
                var nz = ax * by - ay * bx;

                // Normalize(N)
                var ls = nx * nx + ny * ny + nz * nz;
                var invNorm = 1.0f / MathF.Sqrt(ls);

                var normal = new Vector3(
                    nx * invNorm,
                    ny * invNorm,
                    nz * invNorm);

                return new Plane(
                    normal,
                    -(normal.X * point1.X + normal.Y * point1.Y + normal.Z * point1.Z));
            }
        }

        /// <summary>
        /// Creates a new Plane whose normal vector is the source Plane's normal vector normalized.
        /// </summary>
        /// <param name="value">The source Plane.</param>
        /// <returns>The normalized Plane.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane Normalize(Plane value)
        {
            const float FLT_EPSILON = 1.192092896e-07f; // smallest such that 1.0+FLT_EPSILON != 1.0
            if (Vector.IsHardwareAccelerated)
            {
                var normalLengthSquared = value.Normal.LengthSquared();
                if (MathF.Abs(normalLengthSquared - 1.0f) < FLT_EPSILON)
                {
                    // It already normalized, so we don't need to farther process.
                    return value;
                }
                var normalLength = MathF.Sqrt(normalLengthSquared);
                return new Plane(
                    value.Normal / normalLength,
                    value.D / normalLength);
            }
            else
            {
                var f = value.Normal.X * value.Normal.X + value.Normal.Y * value.Normal.Y + value.Normal.Z * value.Normal.Z;

                if (MathF.Abs(f - 1.0f) < FLT_EPSILON)
                {
                    return value; // It already normalized, so we don't need to further process.
                }

                var fInv = 1.0f / MathF.Sqrt(f);

                return new Plane(
                    value.Normal.X * fInv,
                    value.Normal.Y * fInv,
                    value.Normal.Z * fInv,
                    value.D * fInv);
            }
        }

        /// <summary>
        /// Transforms a normalized Plane by a Matrix.
        /// </summary>
        /// <param name="plane"> The normalized Plane to transform. 
        /// This Plane must already be normalized, so that its Normal vector is of unit length, before this method is called.</param>
        /// <param name="matrix">The transformation matrix to apply to the Plane.</param>
        /// <returns>The transformed Plane.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane Transform(Plane plane, Matrix4x4 matrix)
        {
            Matrix4x4.Invert(matrix, out var m);

            float x = plane.Normal.X, y = plane.Normal.Y, z = plane.Normal.Z, w = plane.D;

            return new Plane(
                x * m.M11 + y * m.M12 + z * m.M13 + w * m.M14,
                x * m.M21 + y * m.M22 + z * m.M23 + w * m.M24,
                x * m.M31 + y * m.M32 + z * m.M33 + w * m.M34,
                x * m.M41 + y * m.M42 + z * m.M43 + w * m.M44);
        }

        /// <summary>
        ///  Transforms a normalized Plane by a Quaternion rotation.
        /// </summary>
        /// <param name="plane"> The normalized Plane to transform.
        /// This Plane must already be normalized, so that its Normal vector is of unit length, before this method is called.</param>
        /// <param name="rotation">The Quaternion rotation to apply to the Plane.</param>
        /// <returns>A new Plane that results from applying the rotation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane Transform(Plane plane, Quaternion rotation)
        {
            // Compute rotation matrix.
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

            var m11 = 1.0f - yy2 - zz2;
            var m21 = xy2 - wz2;
            var m31 = xz2 + wy2;

            var m12 = xy2 + wz2;
            var m22 = 1.0f - xx2 - zz2;
            var m32 = yz2 - wx2;

            var m13 = xz2 - wy2;
            var m23 = yz2 + wx2;
            var m33 = 1.0f - xx2 - yy2;

            float x = plane.Normal.X, y = plane.Normal.Y, z = plane.Normal.Z;

            return new Plane(
                x * m11 + y * m21 + z * m31,
                x * m12 + y * m22 + z * m32,
                x * m13 + y * m23 + z * m33,
                plane.D);
        }

        /// <summary>
        /// Calculates the dot product of a Plane and Vector4.
        /// </summary>
        /// <param name="plane">The Plane.</param>
        /// <param name="value">The Vector4.</param>
        /// <returns>The dot product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Plane plane, Vector4 value)
        {
            return plane.Normal.X * value.X +
                   plane.Normal.Y * value.Y +
                   plane.Normal.Z * value.Z +
                   plane.D * value.W;
        }

        /// <summary>
        /// Returns the dot product of a specified Vector3 and the normal vector of this Plane plus the distance (D) value of the Plane.
        /// </summary>
        /// <param name="plane">The plane.</param>
        /// <param name="value">The Vector3.</param>
        /// <returns>The resulting value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DotCoordinate(Plane plane, Vector3 value)
        {
            if (Vector.IsHardwareAccelerated)
            {
                return Vector3.Dot(plane.Normal, value) + plane.D;
            }
            else
            {
                return plane.Normal.X * value.X +
                       plane.Normal.Y * value.Y +
                       plane.Normal.Z * value.Z +
                       plane.D;
            }
        }

        /// <summary>
        /// Returns the dot product of a specified Vector3 and the Normal vector of this Plane.
        /// </summary>
        /// <param name="plane">The plane.</param>
        /// <param name="value">The Vector3.</param>
        /// <returns>The resulting dot product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DotNormal(Plane plane, Vector3 value)
        {
            if (Vector.IsHardwareAccelerated)
            {
                return Vector3.Dot(plane.Normal, value);
            }
            else
            {
                return plane.Normal.X * value.X +
                       plane.Normal.Y * value.Y +
                       plane.Normal.Z * value.Z;
            }
        }

        /// <summary>
        /// Returns a boolean indicating whether the two given Planes are equal.
        /// </summary>
        /// <param name="value1">The first Plane to compare.</param>
        /// <param name="value2">The second Plane to compare.</param>
        /// <returns>True if the Planes are equal; False otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Plane value1, Plane value2)
        {
            return (value1.Normal.X == value2.Normal.X &&
                    value1.Normal.Y == value2.Normal.Y &&
                    value1.Normal.Z == value2.Normal.Z &&
                    value1.D == value2.D);
        }

        /// <summary>
        /// Returns a boolean indicating whether the two given Planes are not equal.
        /// </summary>
        /// <param name="value1">The first Plane to compare.</param>
        /// <param name="value2">The second Plane to compare.</param>
        /// <returns>True if the Planes are not equal; False if they are equal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Plane value1, Plane value2)
        {
            return (value1.Normal.X != value2.Normal.X ||
                    value1.Normal.Y != value2.Normal.Y ||
                    value1.Normal.Z != value2.Normal.Z ||
                    value1.D != value2.D);
        }

        /// <summary>
        /// Returns a boolean indicating whether the given Plane is equal to this Plane instance.
        /// </summary>
        /// <param name="other">The Plane to compare this instance to.</param>
        /// <returns>True if the other Plane is equal to this instance; False otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Plane other)
        {
            if (Vector.IsHardwareAccelerated)
            {
                return Normal.Equals(other.Normal) && D == other.D;
            }
            else
            {
                return (Normal.X == other.Normal.X &&
                        Normal.Y == other.Normal.Y &&
                        Normal.Z == other.Normal.Z &&
                        D == other.D);
            }
        }

        /// <summary>
        /// Returns a boolean indicating whether the given Object is equal to this Plane instance.
        /// </summary>
        /// <param name="obj">The Object to compare against.</param>
        /// <returns>True if the Object is equal to this Plane; False otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is Plane)
            {
                return Equals((Plane)obj);
            }

            return false;
        }

        /// <summary>
        /// Returns a String representing this Plane instance.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            var ci = CultureInfo.CurrentCulture;

            return string.Format(ci, "{{Normal:{0} D:{1}}}", Normal.ToString(), D.ToString(ci));
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return Normal.GetHashCode() + D.GetHashCode();
        }
    }
}

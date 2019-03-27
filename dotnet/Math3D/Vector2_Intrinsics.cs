// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

namespace Ara3D
{
    // This file contains the definitions for all of the JIT intrinsic methods and properties that are recognized by the current x64 JIT compiler.
    // The implementation defined here is used in any circumstance where the JIT fails to recognize these members as intrinsic.
    // The JIT recognizes these methods and properties by name and signature: if either is changed, the JIT will no longer recognize the member.
    // Some methods declared here are not strictly intrinsic, but delegate to an intrinsic method. For example, only one overload of CopyTo() 

    public partial struct Vector2
    {
        /// <summary>
        /// Constructs a vector whose elements are all the single specified value.
        /// </summary>
        [Intrinsic]
        public Vector2(float value) : this(value, value) { }

        /// <summary>
        /// Copies the contents of the vector into the given array.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(float[] array)
            => CopyTo(array, 0);        

        /// <summary>
        /// Copies the contents of the vector into the given array, starting from the given index.
        /// </summary>
        /// <exception cref="ArgumentNullException">If array is null.</exception>
        /// <exception cref="RankException">If array is multidimensional.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If index is greater than end of the array or index is less than zero.</exception>
        /// <exception cref="ArgumentException">If number of elements in source vector is greater than those available in destination array
        /// or if there are not enough elements to copy.</exception>
        public void CopyTo(float[] array, int index)
        {
            if (array == null)
            {
                // Match the JIT's exception type here. For perf, a NullReference is thrown instead of an ArgumentNull.
                throw new NullReferenceException(SR.Arg_NullArgumentNullRef);
            }
            if (index < 0 || index >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), String.Format(SR.Arg_ArgumentOutOfRangeException, index));
            }
            if ((array.Length - index) < 2)
            {
                throw new ArgumentException(String.Format(SR.Arg_ElementsInSourceIsGreaterThanDestination, index));
            }
            array[index] = X;
            array[index + 1] = Y;
        }

        /// <summary>
        /// Returns the dot product of two vectors.
        /// </summary>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Vector2 value1, Vector2 value2)
            => value1.X * value2.X + value1.Y * value2.Y;

        /// <summary>
        /// Returns a vector whose elements are the minimum of each of the pairs of elements in the two source vectors.
        /// </summary>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Min(Vector2 value1, Vector2 value2)
            => new Vector2(Math.Min(value1.X, value2.X), Math.Min(value1.Y, value2.Y));

        /// <summary>
        /// Returns a vector whose elements are the maximum of each of the pairs of elements in the two source vectors
        /// </summary>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Max(Vector2 value1, Vector2 value2)
            => new Vector2(Math.Max(value1.X, value2.X), Math.Max(value1.Y, value2.Y));

        /// <summary>
        /// Returns a vector whose elements are the absolute values of each of the source vector's elements.
        /// </summary>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Abs(Vector2 value)
            => new Vector2(MathF.Abs(value.X), MathF.Abs(value.Y));        

        /// <summary>
        /// Returns a vector whose elements are the square root of each of the source vector's elements.
        /// </summary>
        /// <param name="value">The source vector.</param>
        /// <returns>The square root vector.</returns>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 SquareRoot(Vector2 value)
            => new Vector2(MathF.Sqrt(value.X), MathF.Sqrt(value.Y));

        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator +(Vector2 left, Vector2 right)
            => new Vector2(left.X + right.X, left.Y + right.Y);        

        /// <summary>
        /// Subtracts the second vector from the first.
        /// </summary>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator -(Vector2 left, Vector2 right)
            => new Vector2(left.X - right.X, left.Y - right.Y);        

        /// <summary>
        /// Multiplies two vectors together.
        /// </summary>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(Vector2 left, Vector2 right)
            => new Vector2(left.X * right.X, left.Y * right.Y);        

        /// <summary>
        /// Multiplies a vector by the given scalar.
        /// </summary>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(float left, Vector2 right)
            => new Vector2(left, left) * right;        

        /// <summary>
        /// Multiplies a vector by the given scalar.
        /// </summary>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(Vector2 left, float right)
            => left * new Vector2(right, right);        

        /// <summary>
        /// Divides the first vector by the second.
        /// </summary>
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator /(Vector2 left, Vector2 right)
            => new Vector2(left.X / right.X, left.Y / right.Y);

        /// <summary>
        /// Divides the vector by the given scalar.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator /(Vector2 value1, float value2)
            => value1 / new Vector2(value2);        

        /// <summary>
        /// Negates a given vector.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator -(Vector2 value)
            => Zero - value;        

        /// <summary>
        /// Returns a boolean indicating whether the two given vectors are not equal.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Vector2 left, Vector2 right)
        {
            return !(left == right);
        }
    }
}

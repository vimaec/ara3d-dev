using System;
using System.Collections.Generic;
using System.Numerics;

namespace Ara3D
{
    /// <summary>
    /// Contains extension functions for working with arrays of points or vectors.
    /// </summary>
    public static class PointsExtensions
    {
        public static IArray<float> SampleZeroToOneInclusive(this int count)
        {
            return (count + 1).Select(i => i / (float)count);
        }

        public static IArray<float> SampleZeroToOneExclusive(this int count)
        {
            return count.Select(i => i / (float)count);
        }

        public static IArray<Vector3> InterpolateInclusive(this int count, Func<float, Vector3> function)
        {
            return count.SampleZeroToOneInclusive().Select(function);
        }

        public static IArray<Vector3> Interpolate(this Line self, int count)
        {
            return count.InterpolateInclusive(self.Lerp);
        }

        public static IArray<Vector3> Rotate(this IArray<Vector3> self, Vector3 axis, float angle)
        {
            return self.Transform(Matrix4x4.CreateFromAxisAngle(axis, angle));
        }

        public static IArray<Vector3> Transform(this IArray<Vector3> self, Matrix4x4 matrix)
        {
            return self.Select(x => x.Transform(matrix));
        }
    }
}

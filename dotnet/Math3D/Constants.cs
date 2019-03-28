// MIT License 
// Copyright (C) 2018 Ara 3D. Inc
// Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Ara3D
{
    public static class Constants
    {
        public readonly static Plane XYPlane = new Plane(Vector3.UnitZ, 0);
        public readonly static Plane XZPlane = new Plane(Vector3.UnitY, 0);
        public readonly static Plane YZPlane = new Plane(Vector3.UnitX, 0);

        public const float Pi = (float)Math.PI;
        public const float HalfPi = Pi / 2f;
        public const float TwoPi = Pi * 2f;
        public const float Tolerance = 0.0000001f;
        public const float Log10E = 0.4342945f;
        public const float Log2E = 1.442695f;
        public const float E = (float)Math.E;


        public readonly static Vector3 NaNVector = new Vector3(float.NaN, float.NaN, float.NaN);
        public readonly static Vector3 InfVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        public readonly static Vector3 MaxVector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        public readonly static Vector3 MinVector = new Vector3(float.MinValue, float.MinValue, float.MinValue);
    }
}

﻿// MIT License 
// Copyright (C) 2019 Ara 3D. Inc
// https://ara3d.com
// Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Ara3D
{
    public partial struct Box
    {
        public int Count 
            => 2;

        public Vector3 Center 
            => Min.Average(Max);

        public Vector3[] Corners 
            => GetCorners(new Vector3[8]);

        public static readonly Box Empty 
            = new Box(Vector3.MaxValue, Vector3.MinValue);

        public bool IsEmpty 
            => !IsValid;

        public bool IsValid 
            => Min.X <= Max.X && Min.Y <= Max.Y && Min.Z < Max.Z;

        public Vector3 Extent 
            => Max - Min;

        // Inspired by: https://stackoverflow.com/questions/5254838/calculating-distance-between-a-point-and-a-rectangular-box-nearest-point
        public float Distance(Vector3 point)
            => Vector3.Zero.Max(Min - point).Max(point - Max).Length();

        public float CenterDistance(Vector3 point)
            => Center.Distance(point);

        public Box Translate(Vector3 offset)
            => new Box(Min + offset, Max + offset);

        public Box Merge(Box box)
            => new Box(Min.Min(box.Min), Max.Max(box.Max));

        public Box Merge(Vector3 point)
            => new Box(Min.Min(point), Max.Max(point));

        public float DistanceToOrigin
            => Distance(Vector3.Zero);

        public float CenterDistanceToOrigin
            => CenterDistance(Vector3.Zero);

        public float Volume
            => IsEmpty ? 0 : Extent.ProductComponents();

        public Vector3 this[int n] 
            => n == 0 ? Min : Max;

        public float MaxSide
            => Extent.MaxComponent();

        public float MinSide
            => Extent.MinComponent();

        public ContainmentType Contains(Box box)
        {
            //test if all corner is in the same side of a face by just checking min and max
            if (box.Max.X < Min.X
                || box.Min.X > Max.X
                || box.Max.Y < Min.Y
                || box.Min.Y > Max.Y
                || box.Max.Z < Min.Z
                || box.Min.Z > Max.Z)
                return ContainmentType.Disjoint;

            if (box.Min.X >= Min.X
                && box.Max.X <= Max.X
                && box.Min.Y >= Min.Y
                && box.Max.Y <= Max.Y
                && box.Min.Z >= Min.Z
                && box.Max.Z <= Max.Z)
                return ContainmentType.Contains;

            return ContainmentType.Intersects;
        }

        public ContainmentType Contains(Sphere sphere)
        {
            if (sphere.Center.X - Min.X >= sphere.Radius
                && sphere.Center.Y - Min.Y >= sphere.Radius
                && sphere.Center.Z - Min.Z >= sphere.Radius
                && Max.X - sphere.Center.X >= sphere.Radius
                && Max.Y - sphere.Center.Y >= sphere.Radius
                && Max.Z - sphere.Center.Z >= sphere.Radius)
                return ContainmentType.Contains;

            double dmin = 0;

            double e = sphere.Center.X - Min.X;
            if (e < 0)
            {
                if (e < -sphere.Radius)
                {
                    return ContainmentType.Disjoint;
                }
                dmin += e * e;
            }
            else
            {
                e = sphere.Center.X - Max.X;
                if (e > 0)
                {
                    if (e > sphere.Radius)
                    {
                        return ContainmentType.Disjoint;
                    }
                    dmin += e * e;
                }
            }

            e = sphere.Center.Y - Min.Y;
            if (e < 0)
            {
                if (e < -sphere.Radius)
                {
                    return ContainmentType.Disjoint;
                }
                dmin += e * e;
            }
            else
            {
                e = sphere.Center.Y - Max.Y;
                if (e > 0)
                {
                    if (e > sphere.Radius)
                    {
                        return ContainmentType.Disjoint;
                    }
                    dmin += e * e;
                }
            }

            e = sphere.Center.Z - Min.Z;
            if (e < 0)
            {
                if (e < -sphere.Radius)
                {
                    return ContainmentType.Disjoint;
                }
                dmin += e * e;
            }
            else
            {
                e = sphere.Center.Z - Max.Z;
                if (e > 0)
                {
                    if (e > sphere.Radius)
                    {
                        return ContainmentType.Disjoint;
                    }
                    dmin += e * e;
                }
            }

            if (dmin <= sphere.Radius * sphere.Radius)
                return ContainmentType.Intersects;

            return ContainmentType.Disjoint;
        }

        public bool Contains(Vector3 point)
        {
            return !(point.X < Min.X
                || point.X > Max.X
                || point.Y < Min.Y
                || point.Y > Max.Y
                || point.Z < Min.Z
                || point.Z > Max.Z);
        }

        /// <summary>
        /// Create a bounding box from the given list of points.
        /// </summary>
        public static Box Create(IEnumerable<Vector3> points)
        {
            var minVec = Vector3.MaxValue;
            var maxVec = Vector3.MinValue;
            foreach (var pt in points)
            {
                minVec = minVec.Min(pt);
                maxVec = maxVec.Min(pt);
            }
            return new Box(minVec, maxVec);
        }

        public static Box Create(params Vector3[] points)
            => Create(points.AsEnumerable());
        
        public static Box CreateFromSphere(Sphere sphere)
            => new Box(sphere.Center - new Vector3(sphere.Radius), sphere.Center + new Vector3(sphere.Radius));
        
        /// <summary>
        /// This is the four front corners followed by the four back corners all as if looking from the front
        /// going in clockwise order from upper left. 
        /// </summary>
        public Vector3[] GetCorners(Vector3[] corners)
        {
            if (corners == null)
                throw new ArgumentNullException(nameof(corners));
            if (corners.Length < 8)
                throw new ArgumentOutOfRangeException(nameof(corners));
            // Front
            corners[0] = new Vector3(Min.X, Max.Y, Max.Z);
            corners[1] = new Vector3(Max.X, Max.Y, Max.Z);
            corners[2] = new Vector3(Max.X, Max.Y, Min.Z);
            corners[3] = new Vector3(Min.X, Max.Y, Min.Z);
            // Back
            corners[4] = new Vector3(Min.X, Min.Y, Max.Z);
            corners[5] = new Vector3(Max.X, Min.Y, Max.Z);
            corners[6] = new Vector3(Max.X, Min.Y, Min.Z);
            corners[7] = new Vector3(Min.X, Min.Y, Min.Z);
            return corners;
        }

        public bool Intersects(Box box)
        {
            Intersects(box, out var result);
            return result;
        }

        public void Intersects(Box box, out bool result)
        {
            if ((Max.X >= box.Min.X) && (Min.X <= box.Max.X))
            {
                if ((Max.Y < box.Min.Y) || (Min.Y > box.Max.Y))
                {
                    result = false;
                    return;
                }

                result = (Max.Z >= box.Min.Z) && (Min.Z <= box.Max.Z);
                return;
            }

            result = false;
            return;
        }

        public bool Intersects(Sphere sphere)
        {
            if (sphere.Center.X - Min.X > sphere.Radius
                && sphere.Center.Y - Min.Y > sphere.Radius
                && sphere.Center.Z - Min.Z > sphere.Radius
                && Max.X - sphere.Center.X > sphere.Radius
                && Max.Y - sphere.Center.Y > sphere.Radius
                && Max.Z - sphere.Center.Z > sphere.Radius)
                return true;

            double dmin = 0;

            if (sphere.Center.X - Min.X <= sphere.Radius)
                dmin += (sphere.Center.X - Min.X) * (sphere.Center.X - Min.X);
            else if (Max.X - sphere.Center.X <= sphere.Radius)
                dmin += (sphere.Center.X - Max.X) * (sphere.Center.X - Max.X);

            if (sphere.Center.Y - Min.Y <= sphere.Radius)
                dmin += (sphere.Center.Y - Min.Y) * (sphere.Center.Y - Min.Y);
            else if (Max.Y - sphere.Center.Y <= sphere.Radius)
                dmin += (sphere.Center.Y - Max.Y) * (sphere.Center.Y - Max.Y);

            if (sphere.Center.Z - Min.Z <= sphere.Radius)
                dmin += (sphere.Center.Z - Min.Z) * (sphere.Center.Z - Min.Z);
            else if (Max.Z - sphere.Center.Z <= sphere.Radius)
                dmin += (sphere.Center.Z - Max.Z) * (sphere.Center.Z - Max.Z);

            if (dmin <= sphere.Radius * sphere.Radius)
                return true;

            return false;
        }

        public PlaneIntersectionType Intersects(Plane plane)
        {
            // See http://zach.in.tu-clausthal.de/teaching/cg_literatur/lighthouse3d_view_frustum_culling/index.html

            float ax, ay, az, bx, by, bz;

            if (plane.Normal.X >= 0)
            {
                ax = Max.X;
                bx = Min.X;
            }
            else
            {
                ax = Min.X;
                bx = Max.X;
            }

            if (plane.Normal.Y >= 0)
            {
                ay = Max.Y;
                by = Min.Y;
            }
            else
            {
                ay = Min.Y;
                by = Max.Y;
            }

            if (plane.Normal.Z >= 0)
            {
                az = Max.Z;
                bz = Min.Z;
            }
            else
            {
                az = Min.Z;
                bz = Max.Z;
            }

            // Inline Vector3.Dot(plane.Normal, negativeVertex) + plane.D;
            var distance = plane.Normal.X * bx + plane.Normal.Y * by + plane.Normal.Z * bz + plane.D;
            if (distance > 0)
                return PlaneIntersectionType.Front;

            // Inline Vector3.Dot(plane.Normal, positiveVertex) + plane.D;
            distance = plane.Normal.X * ax + plane.Normal.Y * ay + plane.Normal.Z * az + plane.D;
            if (distance < 0)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }

        public static readonly Box Unit 
            = new Box(Vector3.Zero, new Vector3(1));
    }
}

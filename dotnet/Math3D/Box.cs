﻿// MIT License 
// Copyright (C) 2018 Ara 3D. Inc
// Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

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

        public static readonly Box Empty = new Box(Constants.MaxVector, Constants.MinVector);

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
            var minVec = Constants.MaxVector;
            var maxVec = Constants.MinVector;
            foreach (var ptVector in points)
            {
                minVec.X = (minVec.X < ptVector.X) ? minVec.X : ptVector.X;
                minVec.Y = (minVec.Y < ptVector.Y) ? minVec.Y : ptVector.Y;
                minVec.Z = (minVec.Z < ptVector.Z) ? minVec.Z : ptVector.Z;
                maxVec.X = (maxVec.X > ptVector.X) ? maxVec.X : ptVector.X;
                maxVec.Y = (maxVec.Y > ptVector.Y) ? maxVec.Y : ptVector.Y;
                maxVec.Z = (maxVec.Z > ptVector.Z) ? maxVec.Z : ptVector.Z;
            }
            return new Box(minVec, maxVec);
        }

        public static Box Create(params Vector3[] points)
        {
            return Create(points.AsEnumerable());
        }

        public static Box CreateFromSphere(Sphere sphere)
        {
            return new Box(sphere.Center - new Vector3(sphere.Radius), sphere.Center + new Vector3(sphere.Radius));
        }

        public Vector3[] GetCorners(Vector3[] corners)
        {
            if (corners == null)
                throw new ArgumentNullException(nameof(corners));
            if (corners.Length < 8)
                throw new ArgumentOutOfRangeException(nameof(corners));
            corners[0].X = Min.X; corners[0].Y = Max.Y; corners[0].Z = Max.Z;
            corners[1].X = Max.X; corners[1].Y = Max.Y; corners[1].Z = Max.Z;
            corners[2].X = Max.X; corners[2].Y = Min.Y; corners[2].Z = Max.Z;
            corners[3].X = Min.X; corners[3].Y = Min.Y; corners[3].Z = Max.Z;
            corners[4].X = Min.X; corners[4].Y = Max.Y; corners[4].Z = Min.Z;
            corners[5].X = Max.X; corners[5].Y = Max.Y; corners[5].Z = Min.Z;
            corners[6].X = Max.X; corners[6].Y = Min.Y; corners[6].Z = Min.Z;
            corners[7].X = Min.X; corners[7].Y = Min.Y; corners[7].Z = Min.Z;
            return corners;
        }

        public bool Intersects(Box box)
        {
            Intersects(box, out bool result);
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

            Vector3 positiveVertex;
            Vector3 negativeVertex;

            if (plane.Normal.X >= 0)
            {
                positiveVertex.X = Max.X;
                negativeVertex.X = Min.X;
            }
            else
            {
                positiveVertex.X = Min.X;
                negativeVertex.X = Max.X;
            }

            if (plane.Normal.Y >= 0)
            {
                positiveVertex.Y = Max.Y;
                negativeVertex.Y = Min.Y;
            }
            else
            {
                positiveVertex.Y = Min.Y;
                negativeVertex.Y = Max.Y;
            }

            if (plane.Normal.Z >= 0)
            {
                positiveVertex.Z = Max.Z;
                negativeVertex.Z = Min.Z;
            }
            else
            {
                positiveVertex.Z = Min.Z;
                negativeVertex.Z = Max.Z;
            }

            // Inline Vector3.Dot(plane.Normal, negativeVertex) + plane.D;
            var distance = plane.Normal.X * negativeVertex.X + plane.Normal.Y * negativeVertex.Y + plane.Normal.Z * negativeVertex.Z + plane.D;
            if (distance > 0)
                return PlaneIntersectionType.Front;

            // Inline Vector3.Dot(plane.Normal, positiveVertex) + plane.D;
            distance = plane.Normal.X * positiveVertex.X + plane.Normal.Y * positiveVertex.Y + plane.Normal.Z * positiveVertex.Z + plane.D;
            if (distance < 0)
                return PlaneIntersectionType.Back;

            return PlaneIntersectionType.Intersecting;
        }
    }
}
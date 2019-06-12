﻿// MIT License - Copyright (C) Ara 3D, Inc.
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.


using System;
using System.Collections.Generic;
using System.Numerics;

namespace Ara3D 
{
	public static class ArrayOps
	{
	
		public static IArray< int > Add(this IArray< int > self, IArray< int > other) { return self.Zip(other, MathOps.Add); }
		public static IArray< int > Add(this IArray< int > self, int scalar) { return self.Select(x => MathOps.Add(x, scalar)); }
		public static IArray< int > Add(this int self, IArray< int > vector) { return vector.Select(x => MathOps.Add(self, x)); }
		public static IArray< int > Multiply(this IArray< int > self, IArray< int > other) { return self.Zip(other, MathOps.Multiply); }
		public static IArray< int > Multiply(this IArray< int > self, int scalar) { return self.Select(x => MathOps.Multiply(x, scalar)); }
		public static IArray< int > Multiply(this int self, IArray< int > vector) { return vector.Select(x => MathOps.Multiply(self, x)); }
		public static IArray< int > Subtract(this IArray< int > self, IArray< int > other) { return self.Zip(other, MathOps.Subtract); }
		public static IArray< int > Subtract(this IArray< int > self, int scalar) { return self.Select(x => MathOps.Subtract(x, scalar)); }
		public static IArray< int > Subtract(this int self, IArray< int > vector) { return vector.Select(x => MathOps.Subtract(self, x)); }
		public static IArray< int > Divide(this IArray< int > self, IArray< int > other) { return self.Zip(other, MathOps.Divide); }
		public static IArray< int > Divide(this IArray< int > self, int scalar) { return self.Select(x => MathOps.Divide(x, scalar)); }
		public static IArray< int > Divide(this int self, IArray< int > vector) { return vector.Select(x => MathOps.Divide(self, x)); }
		public static IArray< long > Add(this IArray< long > self, IArray< long > other) { return self.Zip(other, MathOps.Add); }
		public static IArray< long > Add(this IArray< long > self, long scalar) { return self.Select(x => MathOps.Add(x, scalar)); }
		public static IArray< long > Add(this long self, IArray< long > vector) { return vector.Select(x => MathOps.Add(self, x)); }
		public static IArray< long > Multiply(this IArray< long > self, IArray< long > other) { return self.Zip(other, MathOps.Multiply); }
		public static IArray< long > Multiply(this IArray< long > self, long scalar) { return self.Select(x => MathOps.Multiply(x, scalar)); }
		public static IArray< long > Multiply(this long self, IArray< long > vector) { return vector.Select(x => MathOps.Multiply(self, x)); }
		public static IArray< long > Subtract(this IArray< long > self, IArray< long > other) { return self.Zip(other, MathOps.Subtract); }
		public static IArray< long > Subtract(this IArray< long > self, long scalar) { return self.Select(x => MathOps.Subtract(x, scalar)); }
		public static IArray< long > Subtract(this long self, IArray< long > vector) { return vector.Select(x => MathOps.Subtract(self, x)); }
		public static IArray< long > Divide(this IArray< long > self, IArray< long > other) { return self.Zip(other, MathOps.Divide); }
		public static IArray< long > Divide(this IArray< long > self, long scalar) { return self.Select(x => MathOps.Divide(x, scalar)); }
		public static IArray< long > Divide(this long self, IArray< long > vector) { return vector.Select(x => MathOps.Divide(self, x)); }
		public static IArray< float > Add(this IArray< float > self, IArray< float > other) { return self.Zip(other, MathOps.Add); }
		public static IArray< float > Add(this IArray< float > self, float scalar) { return self.Select(x => MathOps.Add(x, scalar)); }
		public static IArray< float > Add(this float self, IArray< float > vector) { return vector.Select(x => MathOps.Add(self, x)); }
		public static IArray< float > Multiply(this IArray< float > self, IArray< float > other) { return self.Zip(other, MathOps.Multiply); }
		public static IArray< float > Multiply(this IArray< float > self, float scalar) { return self.Select(x => MathOps.Multiply(x, scalar)); }
		public static IArray< float > Multiply(this float self, IArray< float > vector) { return vector.Select(x => MathOps.Multiply(self, x)); }
		public static IArray< float > Subtract(this IArray< float > self, IArray< float > other) { return self.Zip(other, MathOps.Subtract); }
		public static IArray< float > Subtract(this IArray< float > self, float scalar) { return self.Select(x => MathOps.Subtract(x, scalar)); }
		public static IArray< float > Subtract(this float self, IArray< float > vector) { return vector.Select(x => MathOps.Subtract(self, x)); }
		public static IArray< float > Divide(this IArray< float > self, IArray< float > other) { return self.Zip(other, MathOps.Divide); }
		public static IArray< float > Divide(this IArray< float > self, float scalar) { return self.Select(x => MathOps.Divide(x, scalar)); }
		public static IArray< float > Divide(this float self, IArray< float > vector) { return vector.Select(x => MathOps.Divide(self, x)); }
		public static IArray< double > Add(this IArray< double > self, IArray< double > other) { return self.Zip(other, MathOps.Add); }
		public static IArray< double > Add(this IArray< double > self, double scalar) { return self.Select(x => MathOps.Add(x, scalar)); }
		public static IArray< double > Add(this double self, IArray< double > vector) { return vector.Select(x => MathOps.Add(self, x)); }
		public static IArray< double > Multiply(this IArray< double > self, IArray< double > other) { return self.Zip(other, MathOps.Multiply); }
		public static IArray< double > Multiply(this IArray< double > self, double scalar) { return self.Select(x => MathOps.Multiply(x, scalar)); }
		public static IArray< double > Multiply(this double self, IArray< double > vector) { return vector.Select(x => MathOps.Multiply(self, x)); }
		public static IArray< double > Subtract(this IArray< double > self, IArray< double > other) { return self.Zip(other, MathOps.Subtract); }
		public static IArray< double > Subtract(this IArray< double > self, double scalar) { return self.Select(x => MathOps.Subtract(x, scalar)); }
		public static IArray< double > Subtract(this double self, IArray< double > vector) { return vector.Select(x => MathOps.Subtract(self, x)); }
		public static IArray< double > Divide(this IArray< double > self, IArray< double > other) { return self.Zip(other, MathOps.Divide); }
		public static IArray< double > Divide(this IArray< double > self, double scalar) { return self.Select(x => MathOps.Divide(x, scalar)); }
		public static IArray< double > Divide(this double self, IArray< double > vector) { return vector.Select(x => MathOps.Divide(self, x)); }
		public static IArray< Vector2 > Add(this IArray< Vector2 > self, IArray< Vector2 > other) { return self.Zip(other, MathOps.Add); }
		public static IArray< Vector2 > Add(this IArray< Vector2 > self, Vector2 scalar) { return self.Select(x => MathOps.Add(x, scalar)); }
		public static IArray< Vector2 > Add(this Vector2 self, IArray< Vector2 > vector) { return vector.Select(x => MathOps.Add(self, x)); }
		public static IArray< Vector2 > Multiply(this IArray< Vector2 > self, IArray< Vector2 > other) { return self.Zip(other, MathOps.Multiply); }
		public static IArray< Vector2 > Multiply(this IArray< Vector2 > self, Vector2 scalar) { return self.Select(x => MathOps.Multiply(x, scalar)); }
		public static IArray< Vector2 > Multiply(this Vector2 self, IArray< Vector2 > vector) { return vector.Select(x => MathOps.Multiply(self, x)); }
		public static IArray< Vector2 > Subtract(this IArray< Vector2 > self, IArray< Vector2 > other) { return self.Zip(other, MathOps.Subtract); }
		public static IArray< Vector2 > Subtract(this IArray< Vector2 > self, Vector2 scalar) { return self.Select(x => MathOps.Subtract(x, scalar)); }
		public static IArray< Vector2 > Subtract(this Vector2 self, IArray< Vector2 > vector) { return vector.Select(x => MathOps.Subtract(self, x)); }
		public static IArray< Vector2 > Divide(this IArray< Vector2 > self, IArray< Vector2 > other) { return self.Zip(other, MathOps.Divide); }
		public static IArray< Vector2 > Divide(this IArray< Vector2 > self, Vector2 scalar) { return self.Select(x => MathOps.Divide(x, scalar)); }
		public static IArray< Vector2 > Divide(this Vector2 self, IArray< Vector2 > vector) { return vector.Select(x => MathOps.Divide(self, x)); }
		public static IArray< Vector3 > Add(this IArray< Vector3 > self, IArray< Vector3 > other) { return self.Zip(other, MathOps.Add); }
		public static IArray< Vector3 > Add(this IArray< Vector3 > self, Vector3 scalar) { return self.Select(x => MathOps.Add(x, scalar)); }
		public static IArray< Vector3 > Add(this Vector3 self, IArray< Vector3 > vector) { return vector.Select(x => MathOps.Add(self, x)); }
		public static IArray< Vector3 > Multiply(this IArray< Vector3 > self, IArray< Vector3 > other) { return self.Zip(other, MathOps.Multiply); }
		public static IArray< Vector3 > Multiply(this IArray< Vector3 > self, Vector3 scalar) { return self.Select(x => MathOps.Multiply(x, scalar)); }
		public static IArray< Vector3 > Multiply(this Vector3 self, IArray< Vector3 > vector) { return vector.Select(x => MathOps.Multiply(self, x)); }
		public static IArray< Vector3 > Subtract(this IArray< Vector3 > self, IArray< Vector3 > other) { return self.Zip(other, MathOps.Subtract); }
		public static IArray< Vector3 > Subtract(this IArray< Vector3 > self, Vector3 scalar) { return self.Select(x => MathOps.Subtract(x, scalar)); }
		public static IArray< Vector3 > Subtract(this Vector3 self, IArray< Vector3 > vector) { return vector.Select(x => MathOps.Subtract(self, x)); }
		public static IArray< Vector3 > Divide(this IArray< Vector3 > self, IArray< Vector3 > other) { return self.Zip(other, MathOps.Divide); }
		public static IArray< Vector3 > Divide(this IArray< Vector3 > self, Vector3 scalar) { return self.Select(x => MathOps.Divide(x, scalar)); }
		public static IArray< Vector3 > Divide(this Vector3 self, IArray< Vector3 > vector) { return vector.Select(x => MathOps.Divide(self, x)); }
		public static IArray< Vector4 > Add(this IArray< Vector4 > self, IArray< Vector4 > other) { return self.Zip(other, MathOps.Add); }
		public static IArray< Vector4 > Add(this IArray< Vector4 > self, Vector4 scalar) { return self.Select(x => MathOps.Add(x, scalar)); }
		public static IArray< Vector4 > Add(this Vector4 self, IArray< Vector4 > vector) { return vector.Select(x => MathOps.Add(self, x)); }
		public static IArray< Vector4 > Multiply(this IArray< Vector4 > self, IArray< Vector4 > other) { return self.Zip(other, MathOps.Multiply); }
		public static IArray< Vector4 > Multiply(this IArray< Vector4 > self, Vector4 scalar) { return self.Select(x => MathOps.Multiply(x, scalar)); }
		public static IArray< Vector4 > Multiply(this Vector4 self, IArray< Vector4 > vector) { return vector.Select(x => MathOps.Multiply(self, x)); }
		public static IArray< Vector4 > Subtract(this IArray< Vector4 > self, IArray< Vector4 > other) { return self.Zip(other, MathOps.Subtract); }
		public static IArray< Vector4 > Subtract(this IArray< Vector4 > self, Vector4 scalar) { return self.Select(x => MathOps.Subtract(x, scalar)); }
		public static IArray< Vector4 > Subtract(this Vector4 self, IArray< Vector4 > vector) { return vector.Select(x => MathOps.Subtract(self, x)); }
		public static IArray< Vector4 > Divide(this IArray< Vector4 > self, IArray< Vector4 > other) { return self.Zip(other, MathOps.Divide); }
		public static IArray< Vector4 > Divide(this IArray< Vector4 > self, Vector4 scalar) { return self.Select(x => MathOps.Divide(x, scalar)); }
		public static IArray< Vector4 > Divide(this Vector4 self, IArray< Vector4 > vector) { return vector.Select(x => MathOps.Divide(self, x)); }

        public static IArray<double> Abs (this IArray< double > self) { return self.Select(MathOps.Abs); }
        public static IArray<float> Abs (this IArray< float > self) { return self.Select(MathOps.Abs); }
        public static IArray<Vector2> Abs (this IArray< Vector2 > self) { return self.Select(MathOps.Abs); }
        public static IArray<Vector3> Abs (this IArray< Vector3 > self) { return self.Select(MathOps.Abs); }
        public static IArray<Vector4> Abs (this IArray< Vector4 > self) { return self.Select(MathOps.Abs); }

        public static IArray<double> Acos (this IArray< double > self) { return self.Select(MathOps.Acos); }
        public static IArray<float> Acos (this IArray< float > self) { return self.Select(MathOps.Acos); }
        public static IArray<Vector2> Acos (this IArray< Vector2 > self) { return self.Select(MathOps.Acos); }
        public static IArray<Vector3> Acos (this IArray< Vector3 > self) { return self.Select(MathOps.Acos); }
        public static IArray<Vector4> Acos (this IArray< Vector4 > self) { return self.Select(MathOps.Acos); }

        public static IArray<double> Asin (this IArray< double > self) { return self.Select(MathOps.Asin); }
        public static IArray<float> Asin (this IArray< float > self) { return self.Select(MathOps.Asin); }
        public static IArray<Vector2> Asin (this IArray< Vector2 > self) { return self.Select(MathOps.Asin); }
        public static IArray<Vector3> Asin (this IArray< Vector3 > self) { return self.Select(MathOps.Asin); }
        public static IArray<Vector4> Asin (this IArray< Vector4 > self) { return self.Select(MathOps.Asin); }

        public static IArray<double> Atan (this IArray< double > self) { return self.Select(MathOps.Atan); }
        public static IArray<float> Atan (this IArray< float > self) { return self.Select(MathOps.Atan); }
        public static IArray<Vector2> Atan (this IArray< Vector2 > self) { return self.Select(MathOps.Atan); }
        public static IArray<Vector3> Atan (this IArray< Vector3 > self) { return self.Select(MathOps.Atan); }
        public static IArray<Vector4> Atan (this IArray< Vector4 > self) { return self.Select(MathOps.Atan); }

        public static IArray<double> Cos (this IArray< double > self) { return self.Select(MathOps.Cos); }
        public static IArray<float> Cos (this IArray< float > self) { return self.Select(MathOps.Cos); }
        public static IArray<Vector2> Cos (this IArray< Vector2 > self) { return self.Select(MathOps.Cos); }
        public static IArray<Vector3> Cos (this IArray< Vector3 > self) { return self.Select(MathOps.Cos); }
        public static IArray<Vector4> Cos (this IArray< Vector4 > self) { return self.Select(MathOps.Cos); }

        public static IArray<double> Cosh (this IArray< double > self) { return self.Select(MathOps.Cosh); }
        public static IArray<float> Cosh (this IArray< float > self) { return self.Select(MathOps.Cosh); }
        public static IArray<Vector2> Cosh (this IArray< Vector2 > self) { return self.Select(MathOps.Cosh); }
        public static IArray<Vector3> Cosh (this IArray< Vector3 > self) { return self.Select(MathOps.Cosh); }
        public static IArray<Vector4> Cosh (this IArray< Vector4 > self) { return self.Select(MathOps.Cosh); }

        public static IArray<double> Exp (this IArray< double > self) { return self.Select(MathOps.Exp); }
        public static IArray<float> Exp (this IArray< float > self) { return self.Select(MathOps.Exp); }
        public static IArray<Vector2> Exp (this IArray< Vector2 > self) { return self.Select(MathOps.Exp); }
        public static IArray<Vector3> Exp (this IArray< Vector3 > self) { return self.Select(MathOps.Exp); }
        public static IArray<Vector4> Exp (this IArray< Vector4 > self) { return self.Select(MathOps.Exp); }

        public static IArray<double> Log (this IArray< double > self) { return self.Select(MathOps.Log); }
        public static IArray<float> Log (this IArray< float > self) { return self.Select(MathOps.Log); }
        public static IArray<Vector2> Log (this IArray< Vector2 > self) { return self.Select(MathOps.Log); }
        public static IArray<Vector3> Log (this IArray< Vector3 > self) { return self.Select(MathOps.Log); }
        public static IArray<Vector4> Log (this IArray< Vector4 > self) { return self.Select(MathOps.Log); }

        public static IArray<double> Log10 (this IArray< double > self) { return self.Select(MathOps.Log10); }
        public static IArray<float> Log10 (this IArray< float > self) { return self.Select(MathOps.Log10); }
        public static IArray<Vector2> Log10 (this IArray< Vector2 > self) { return self.Select(MathOps.Log10); }
        public static IArray<Vector3> Log10 (this IArray< Vector3 > self) { return self.Select(MathOps.Log10); }
        public static IArray<Vector4> Log10 (this IArray< Vector4 > self) { return self.Select(MathOps.Log10); }

        public static IArray<double> Sin (this IArray< double > self) { return self.Select(MathOps.Sin); }
        public static IArray<float> Sin (this IArray< float > self) { return self.Select(MathOps.Sin); }
        public static IArray<Vector2> Sin (this IArray< Vector2 > self) { return self.Select(MathOps.Sin); }
        public static IArray<Vector3> Sin (this IArray< Vector3 > self) { return self.Select(MathOps.Sin); }
        public static IArray<Vector4> Sin (this IArray< Vector4 > self) { return self.Select(MathOps.Sin); }

        public static IArray<double> Sinh (this IArray< double > self) { return self.Select(MathOps.Sinh); }
        public static IArray<float> Sinh (this IArray< float > self) { return self.Select(MathOps.Sinh); }
        public static IArray<Vector2> Sinh (this IArray< Vector2 > self) { return self.Select(MathOps.Sinh); }
        public static IArray<Vector3> Sinh (this IArray< Vector3 > self) { return self.Select(MathOps.Sinh); }
        public static IArray<Vector4> Sinh (this IArray< Vector4 > self) { return self.Select(MathOps.Sinh); }

        public static IArray<double> Sqrt (this IArray< double > self) { return self.Select(MathOps.Sqrt); }
        public static IArray<float> Sqrt (this IArray< float > self) { return self.Select(MathOps.Sqrt); }
        public static IArray<Vector2> Sqrt (this IArray< Vector2 > self) { return self.Select(MathOps.Sqrt); }
        public static IArray<Vector3> Sqrt (this IArray< Vector3 > self) { return self.Select(MathOps.Sqrt); }
        public static IArray<Vector4> Sqrt (this IArray< Vector4 > self) { return self.Select(MathOps.Sqrt); }

        public static IArray<double> Tan (this IArray< double > self) { return self.Select(MathOps.Tan); }
        public static IArray<float> Tan (this IArray< float > self) { return self.Select(MathOps.Tan); }
        public static IArray<Vector2> Tan (this IArray< Vector2 > self) { return self.Select(MathOps.Tan); }
        public static IArray<Vector3> Tan (this IArray< Vector3 > self) { return self.Select(MathOps.Tan); }
        public static IArray<Vector4> Tan (this IArray< Vector4 > self) { return self.Select(MathOps.Tan); }

        public static IArray<double> Tanh (this IArray< double > self) { return self.Select(MathOps.Tanh); }
        public static IArray<float> Tanh (this IArray< float > self) { return self.Select(MathOps.Tanh); }
        public static IArray<Vector2> Tanh (this IArray< Vector2 > self) { return self.Select(MathOps.Tanh); }
        public static IArray<Vector3> Tanh (this IArray< Vector3 > self) { return self.Select(MathOps.Tanh); }
        public static IArray<Vector4> Tanh (this IArray< Vector4 > self) { return self.Select(MathOps.Tanh); }

        public static IArray<double> Sqr (this IArray< double > self) { return self.Select(MathOps.Sqr); }
        public static IArray<float> Sqr (this IArray< float > self) { return self.Select(MathOps.Sqr); }
        public static IArray<Vector2> Sqr (this IArray< Vector2 > self) { return self.Select(MathOps.Sqr); }
        public static IArray<Vector3> Sqr (this IArray< Vector3 > self) { return self.Select(MathOps.Sqr); }
        public static IArray<Vector4> Sqr (this IArray< Vector4 > self) { return self.Select(MathOps.Sqr); }

        public static IArray<double> Inverse (this IArray< double > self) { return self.Select(MathOps.Inverse); }
        public static IArray<float> Inverse (this IArray< float > self) { return self.Select(MathOps.Inverse); }
        public static IArray<Vector2> Inverse (this IArray< Vector2 > self) { return self.Select(MathOps.Inverse); }
        public static IArray<Vector3> Inverse (this IArray< Vector3 > self) { return self.Select(MathOps.Inverse); }
        public static IArray<Vector4> Inverse (this IArray< Vector4 > self) { return self.Select(MathOps.Inverse); }

        public static IArray<double> Ceiling (this IArray< double > self) { return self.Select(MathOps.Ceiling); }
        public static IArray<float> Ceiling (this IArray< float > self) { return self.Select(MathOps.Ceiling); }
        public static IArray<Vector2> Ceiling (this IArray< Vector2 > self) { return self.Select(MathOps.Ceiling); }
        public static IArray<Vector3> Ceiling (this IArray< Vector3 > self) { return self.Select(MathOps.Ceiling); }
        public static IArray<Vector4> Ceiling (this IArray< Vector4 > self) { return self.Select(MathOps.Ceiling); }

        public static IArray<double> Floor (this IArray< double > self) { return self.Select(MathOps.Floor); }
        public static IArray<float> Floor (this IArray< float > self) { return self.Select(MathOps.Floor); }
        public static IArray<Vector2> Floor (this IArray< Vector2 > self) { return self.Select(MathOps.Floor); }
        public static IArray<Vector3> Floor (this IArray< Vector3 > self) { return self.Select(MathOps.Floor); }
        public static IArray<Vector4> Floor (this IArray< Vector4 > self) { return self.Select(MathOps.Floor); }

        public static IArray<double> Round (this IArray< double > self) { return self.Select(MathOps.Round); }
        public static IArray<float> Round (this IArray< float > self) { return self.Select(MathOps.Round); }
        public static IArray<Vector2> Round (this IArray< Vector2 > self) { return self.Select(MathOps.Round); }
        public static IArray<Vector3> Round (this IArray< Vector3 > self) { return self.Select(MathOps.Round); }
        public static IArray<Vector4> Round (this IArray< Vector4 > self) { return self.Select(MathOps.Round); }

        public static IArray<double> Truncate (this IArray< double > self) { return self.Select(MathOps.Truncate); }
        public static IArray<float> Truncate (this IArray< float > self) { return self.Select(MathOps.Truncate); }
        public static IArray<Vector2> Truncate (this IArray< Vector2 > self) { return self.Select(MathOps.Truncate); }
        public static IArray<Vector3> Truncate (this IArray< Vector3 > self) { return self.Select(MathOps.Truncate); }
        public static IArray<Vector4> Truncate (this IArray< Vector4 > self) { return self.Select(MathOps.Truncate); }

		public static long Sum(this IArray<int> self) { return self.Aggregate(0L, (x, y) => x + y); }
		public static long Sum(this IArray<long> self) { return self.Aggregate(0L, (x, y) => x + y); }
		public static double Sum(this IArray<float> self) { return self.Aggregate(0.0, (x, y) => x + y); }
		public static double Sum(this IArray<double> self) { return self.Aggregate(0.0, (x, y) => x + y); }
		public static Vector2 Sum(this IArray<Vector2> self) { return self.Aggregate(Vector2.Zero, (x, y) => x + y); }
		public static Vector3 Sum(this IArray<Vector3> self) { return self.Aggregate(Vector3.Zero, (x, y) => x + y); }
		public static Vector4 Sum(this IArray<Vector4> self) { return self.Aggregate(Vector4.Zero, (x, y) => x + y); }
		
		public static double Average(this IArray<int> self) { return self.Sum() / self.Count; }
		public static double Average(this IArray<long> self) { return self.Sum() / self.Count; }
		public static double Average(this IArray<float> self) { return self.Sum() / self.Count; }
		public static double Average(this IArray<double> self) { return self.Sum() / self.Count; }
		public static Vector2 Average(this IArray<Vector2> self) { return self.Sum() / self.Count; }
		public static Vector3 Average(this IArray<Vector3> self) { return self.Sum() / self.Count; }
		public static Vector4 Average(this IArray<Vector4> self) { return self.Sum() / self.Count; }	

		public static double Variance(this IArray<int> self) { var mean = self.Average(); return self.Select(x => MathOps.Sqr(x - mean)).Average(); }
		public static double Variance(this IArray<long> self) { var mean = self.Average(); return self.Select(x => MathOps.Sqr(x - mean)).Average(); }
		public static double Variance(this IArray<float> self) { var mean = self.Average(); return self.Select(x => MathOps.Sqr(x - mean)).Average(); }
		public static double Variance(this IArray<double> self) { var mean = self.Average(); return self.Select(x => MathOps.Sqr(x - mean)).Average(); }
		public static Vector2 Variance(this IArray<Vector2> self) { var mean = self.Average(); return self.Select(x => MathOps.Sqr(x - mean)).Average(); }
		public static Vector3 Variance(this IArray<Vector3> self) { var mean = self.Average(); return self.Select(x => MathOps.Sqr(x - mean)).Average(); }
		public static Vector4 Variance(this IArray<Vector4> self) { var mean = self.Average(); return self.Select(x => MathOps.Sqr(x - mean)).Average(); }

		public static double StdDev(this IArray<int> self) { return self.Variance().Sqrt(); }
		public static double StdDev(this IArray<long> self) { return self.Variance().Sqrt(); }
		public static double StdDev(this IArray<float> self) { return self.Variance().Sqrt(); }
		public static double StdDev(this IArray<double> self) { return self.Variance().Sqrt(); }
		public static Vector2 StdDev(this IArray<Vector2> self) { return self.Variance().Sqrt(); }
		public static Vector3 StdDev(this IArray<Vector3> self) { return self.Variance().Sqrt(); }
		public static Vector4 StdDev(this IArray<Vector4> self) { return self.Variance().Sqrt(); }

		public static IArray<int> PartialSums(this IArray<int> self) { return self.Accumulate((x, y) => x + y); }
		public static IArray<long> PartialSums(this IArray<long> self) { return self.Accumulate((x, y) => x + y); }
		public static IArray<float> PartialSums(this IArray<float> self) { return self.Accumulate((x, y) => x + y); }
		public static IArray<double> PartialSums(this IArray<double> self) { return self.Accumulate((x, y) => x + y); }
		public static IArray<Vector2> PartialSums(this IArray<Vector2> self) { return self.Accumulate((x, y) => x + y); }
		public static IArray<Vector3> PartialSums(this IArray<Vector3> self) { return self.Accumulate((x, y) => x + y); }
		public static IArray<Vector4> PartialSums(this IArray<Vector4> self) { return self.Accumulate((x, y) => x + y); }
		
	} 
} 

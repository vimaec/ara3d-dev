# Ara3D.Math

**Ara3D.Math** is a portable high-performance 3D math library from [Ara3D](https://ara3d.com) written in pure C# with no 
dependencies. Math3D was forked from the core classes provided in the CoreFX implementation of 
[System.Numerics](https://github.com/dotnet/corefx/tree/master/src/System.Numerics.Vectors/src/System/Numerics) and 
[MonoGame](https://github.com/MonoGame/MonoGame) an open-source version of XNA.

## Why not use System.Numerics

Originally Ara3D used `System.Numerics` directly but we ran into several issues:

* The layout in memory of `System.Numerics.Vector3` is different between .NET Framework and .NET Core
* The structs are mutable, violating Microsoft's own best practices around [struct design](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/struct).
* There are no double precision versions of the structs, so we had to create our own
* Many useful functions were only available as static non-extension functions, forcing users to use a non-fluent API
* The number of structs and functions was limited, so we had to extend them anyway
* System.Numerics used some unsafe code in places 

Because System.Numerics is based on very well-known and proven algorithms that arent likely to change, we found it 
to be a reasonable approach to fork the code, and put it into a stsandalone library 

## Why not use Unity.Mathematics

In the last year Unity released a prototype library called [`Unity.Mathematics`](https://github.com/Unity-Technologies/Unity.Mathematics).
The reasons this library are not used:

* Unity.Mathematics is not using a widely recognized open-source license 
* The API is a work in progress and we may introduce breaking changes
* The API supports a very limited set of types and operations (basically that of GLSL)
* The library nomenclature and style is based on GLSL rather than C# System Libraries

## What Structs are Provided

The following is a list of data structure that are provide by Ara3D.Math

	* Ara3D.Vector2
	* Ara3D.Vector3
	* Ara3D.Vector4	
	* Ara3D.DVector2
	* Ara3D.DVector3
	* Ara3D.DVector4
	* Ara3D.Plane
	* Ara3D.DPlane
	* Ara3D.Quaternion
	* Ara3D.DQuaternion
	* Ara3D.Interval
	* Ara3D.Box2
	* Ara3D.Box
	* Ara3D.Box4
	* Ara3D.DInterval
	* Ara3D.DBox2
	* Ara3D.DBox3
	* Ara3D.DBox4
	* Ara3D.Ray
	* Ara3D.DRay
	* Ara3D.Sphere
	* Ara3D.DSphere
	* Ara3D.Line
	* Ara3D.Triangle
	* Ara3D.Triangle2
	* Ara3D.Quad
	* Ara3D.Int2
	* Ara3D.Int3
	* Ara3D.Int4

## System.Math as Extension Functions, 

All of the `System.Math` routines, and additional math routines, are reimplemented in `Ara3D.MathOps` as 
static extension functions for `float`, `double`, `Vector2`,`Vector3`, `Vector4`, `DVector2`,`DVector3`, 
and `DVector4`. This provides a convenient fluent syntax on all variables. 

## What are .TT Files

`Ara3D.Math` leverages the [T4 text template engine](https://docs.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates?view=vs-2017) 
to auto-generate efficient boilerplate code for the different types of 
structs. This is how Microsoft implemented the System.Numerics library and has proven for us to be an effective way to 
create efficient generic code for numerical types. 

## Related Libraries 

* [Math.NET Numerics](https://github.com/mathnet/mathnet-numerics)
* [Geometry3Sharp](https://github.com/gradientspace/geometry3Sharp)
* [MonoGame](https://github.com/MonoGame/MonoGame)
* [FNA-XNA](https://github.com/FNA-XNA/FNA/tree/master/src)
* [Xenko](https://github.com/xenko3d/xenko/blob/master/sources/core/Xenko.Core.Mathematics)
* [Unity.Mathematics](https://github.com/Unity-Technologies/Unity.Mathematics)
* [System.Numerics - Reference Source](https://referencesource.microsoft.com/#System.Numerics,namespaces)
* [System.Numerics - CoreFX](https://github.com/dotnet/corefx/tree/master/src/System.Numerics.Vectors/src/System/Numerics)
* [Unity Reference](https://github.com/Unity-Technologies/UnityCsReference/tree/master/Runtime/Export)
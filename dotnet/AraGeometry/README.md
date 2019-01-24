# FGeometry

**FGeometry** is an efficient open-source cross-platform .NET library of pure functional 3D geometry data structures and algorithms inspired by LINQ and 3ds Max MCG.

## Why Functional Programming? 

There are many advantages to functional programming, and in particular the usage of immutable data structures and lazy evaluation. Some of these are: 

* Easier parallelization
* Simpler algorithms
* Fewer memory allocations  
* Less defects
* Easier refactoring
* Faster prototyping

### No More Triangle Soup

Many geometry libraries and algorithms work on either solid bodies, winged-edge data structures, or some form of triangle soup. While FGeometry library works great on triangle soup, one big advantage is that it allows the same manipulations to be performed on functional representations of solid bodies or other procedural representations of geometry, and to the tesselation at the last possible minute if needed at all.

## Related Work

FGeometry has some similarities to the 3D libraries in Geometry3D.dll used in the Max Creation Graph visual programming extension to Autodesk 3ds Max.

The FGeometry library has a number of distinguishing features, among them include: 

* Cross platform 
* Scene graph
* Memoization of expensive data structures
* Support for polygon meshes 
* Uses a simpler array interface
* Uses SIMD accelerated math primitives 
* No bindings to 3ds Max data structures 
* Smart sub-element data structures
* Minimal dependencies 
* Open-source

Another popular library for 3D Geomtry manipulation is the excellent [Geometry3Sharp](https://github.com/gradientspace/geometry3Sharp) library by Ryan Schmidt from [GradientSpace](https://github.com/gradientspace). 

Geometry3Sharp contains a large number of useful and interesting algorithms, but because is designed more for the 3D printing market, so makes some perforamnce trade-offs. We plan on creating a bridge to Geometry3Sharp in the future, so we can get the best of both worlds.  

The [MonoGame library](https://github.com/MonoGame/MonoGame) has a number of useful low-level 3D algorithms and data structures. We use some modified MonoGame code in the Math3D library. It is interesting to note that System.Numerics and MonoGame have some common ancestroy in the now discontinued XNA library.  

## Platforms 

The FGeometry library targets [.NET Standard 2.0](https://docs.microsoft.com/en-us/dotnet/standard/net-standard), which means it is compatible with:

* Unity 2018.1 
* .NET Core	2.0
* .NET Framework 4.6.1
* Mono 5.4
* Xamarin.iOS 10.14
* Xamarin.Mac 3.8
* Xamarin.Android 8.0
* Universal Windows Platform 10.0.16299

## Dependencies
g
FGeometry is built on top of:

* [Ara3D.LinqArray](https://github.com/ara3d/LinqArray) - LINQ for Arrays
* [Ara3D.Math3D](https://github.com/ara3d/Math3D) - Efficient 3D Math libraries for C#  
* [Ara3D.CSharpOverflow](https://github.com/ara3d/CSharpOverflow) - Extension methods, helper classes, and utilities for C#.

These libraries are not yet released as NuGet packages, so the recommended approach is to clocdne each repository. 

## Status

This library is under active development for use in products being developed by Ara 3D. 




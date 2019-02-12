using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Ara3D
{    
    public interface IPropertySetDescriptor
    {
        int Count { get; }
        IPropertySet this[int n] { get; }
    }

    public interface IPropertyDescriptor
    {
        Guid Id { get; }
        Type Type { get; }
        string Name { get; }
        int Index { get; }
        string Descriptor { get; }
    }

    public interface IPropertySet
    {
        int Count { get; }
        object this[int n] { get; }
        IPropertySetDescriptor Descriptors { get; }
    }

    public interface IMaterial
    {
        IPropertySet Properties { get; }
    }

    public interface ISceneObject
    {
        IPropertySet Properties { get; }
        Matrix4x4 Transform { get; }
        string Id { get; }
        string ParentId { get; }
        string Name { get; }
    }

    public interface IGeometryObject : ISceneObject
    {
        IGeometry Geometry { get; }
        IMaterial Material { get; }
    }

    public interface ILightObject : ISceneObject
    { }

    public interface ICameraObject : ISceneObject
    { }

    public interface ILayerObject : ISceneObject
    { }

    public interface IGroupObject : ISceneObject
    { }

    public interface IReferenceObject : ISceneObject
    {
        string Uri { get; }
    }

    public interface IUriResolver
    {
        ISceneObject Resolve(string uri);
    }

    public interface IScene : ISceneObject
    {
        IArray<IReferenceObject> References { get; }
        IArray<ICameraObject> Cameras { get; }
        IArray<ILightObject> Lights { get; }
        IArray<IGeometryObject> Geometries { get; }
        IArray<ILayerObject> Layers { get; }
        IArray<IGroupObject> Groups { get; }
    }
}

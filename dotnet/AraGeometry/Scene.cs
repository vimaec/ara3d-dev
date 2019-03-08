using System;
using System.Numerics;

namespace Ara3D
{
    public interface ISceneNode
    {
        Matrix4x4 Transform { get; }
        int GeometryId { get; }
        int ElementId { get; }
        int MaterialId { get; }
        int CategoryId { get; }
    }

    public interface ISceneObject
    {
        IScene Scene { get; }
        ISceneNode Node { get; }
    }

    public interface IScene
    {
        IArray<IGeometry> Geometries { get; }
        IArray<ISceneObject> Objects { get; }
    }

    public class SceneNode : ISceneNode
    {
        public SceneNode(string name, int geometryId, Matrix4x4 transform, int elementId, int materialId)
        {
            Name = name;
            GeometryId = geometryId;
            Transform = transform;
            ElementId = elementId;
            MaterialId = materialId;
        }

        public int GeometryId { get; }
        public string Name { get; }
        public Matrix4x4 Transform { get; }
        public int MaterialId { get; set; }
        public int CategoryId { get; set; }
        public int ElementId { get; set; }
    }

    public class Scene : IScene
    {
        public Scene(IArray<IGeometry> geometries, IArray<ISceneNode> nodes)
        {
            Geometries = geometries;
            Objects = nodes.Select(n => new SceneObject(this, n) as ISceneObject);
        }
        public IArray<IGeometry> Geometries { get; }
        public IArray<ISceneObject> Objects { get; }
    }

    public class SceneObject : ISceneObject
    {
        public SceneObject(IScene scene, ISceneNode node)
        {
            Scene = scene;
            Node = node;
        }
        public IScene Scene { get; }
        public ISceneNode Node { get; }
    }

    // TODO: throw this out eventually, only exists for simplification of JSON serialization. 
    public class ManifestSceneNode 
    {
        public float[] Transform { get; set; }
        public int ElementId { get; set; }
        public int GeometryId { get; set; }
        public int MaterialId { get; set; }        
    }

    public static class SceneExtensions
    {
        public static IGeometry Geometry(this ISceneObject obj)
            => obj.Scene.Geometries[obj.Node.GeometryId];

        public static IGeometry TransformedGeometry(this ISceneObject obj)
            => obj.Geometry().Transform(obj.Node.Transform);

        public static IArray<IGeometry> TransformedGeometries(this IScene scene)
            => scene.Objects.Select(TransformedGeometry);

        public static IGeometry ToIGeometry(this IScene scene)
            => scene.TransformedGeometries().Merge();
    }
}

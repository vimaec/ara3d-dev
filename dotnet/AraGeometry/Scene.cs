using System.Numerics;

namespace Ara3D
{    
    public interface ISceneObject
    {
        Matrix4x4 Transform { get; }
        int Id { get; }
        int ParentId { get; }
        int GeometryId { get; }
        string Name { get; }
    }

    public interface IScene
    { 
        IArray<IGeometry> Geometries { get; }
        IArray<ISceneObject> Objects { get; }
    }

    public static class SceneExtensions
    {
        public static IGeometry TransformedGeometry(this IScene scene, ISceneObject obj)
            => scene.Geometries[obj.GeometryId].Transform(obj.Transform);

        public static IArray<IGeometry> TransformedGeometries(this IScene scene)
            => scene.Objects.Select(scene.TransformedGeometry);

        public static IGeometry ToIGeometry(this IScene scene)
            => scene.TransformedGeometries().Merge();
    }
}

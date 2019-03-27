using System;
using System.Numerics;
using System.Threading.Tasks;
using Ara3D.DotNetUtilities;

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
        IFormatLoader Loader { get; }
        IArray<ISceneObject> Objects { get; }

        Task<IGeometry> LoadGeometry(int id);
    }

    public class SceneNode : ISceneNode
    {
        public SceneNode(string name, int geometryId, Matrix4x4 transform, int elementId, int materialId, int categoryId)
        {
            Name = name;
            GeometryId = geometryId;
            Transform = transform;
            ElementId = elementId;
            MaterialId = materialId;
            CategoryId = categoryId;
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
        public Scene(IArray<ISceneNode> nodes, IFormatLoader resourceLoader)
        {
            Objects = nodes.Select(n => new SceneObject(this, n) as ISceneObject);
            Loader = resourceLoader;
        }
        public IFormatLoader Loader { get; }
        public IArray<ISceneObject> Objects { get; }

        async Task<IGeometry> IScene.LoadGeometry(int id)
        {
            var bytes = await Loader.ResourceGeometryAsync(id);
            var geo = (bytes != null) ? G3D.Create(bytes).ToIGeometry() : null;
            return geo;
        }
        // TODO - perhaps push material loading back into this class?
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
    [Serializable]
    public class ManifestSceneNode
    {
        public float[] Transform { get; set; }
        public int ElementId { get; set; }
        public int GeometryId { get; set; }
        public int MaterialId { get; set; }
        public int CategoryId { get; set; }
    }

    public static class SceneExtensions
    {
        public static Task<IGeometry> Geometry(this ISceneObject obj)
            => obj.Scene.LoadGeometry(obj.Node.GeometryId);

        public static async Task<IGeometry> TransformedGeometry(this ISceneObject obj)
            => (await obj.Geometry()).Transform(obj.Node.Transform);

        // TODO: Perhaps add a function to trigger loading all geometries?
        //public static async IArray<IGeometry> TransformedGeometries(this IScene scene)
        //    => scene.Objects.Select(TransformedGeometry);

        //public static IGeometry ToIGeometry(this IScene scene)
        //    => scene.TransformedGeometries().Merge();
    }
}

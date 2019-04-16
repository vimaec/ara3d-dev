namespace Ara3D
{
    public interface ISceneNode
    {
        IScene Scene { get; }
        int Index { get; }
        Matrix4x4 Transform { get; }
        int GeometryId { get; }
        int ParentIndex { get; }
    }

    public interface IScene
    {
        IArray<IGeometry> Geometries { get; }
        IArray<ISceneNode> Nodes { get; }
        IArray<ISceneNode> ChildNodes(int n);
    }

    public static class SceneExtensions
    {
        public static IGeometry Geometry(this ISceneNode node)
            => node.GeometryId >= 0 ? node.Scene.Geometries[node.GeometryId] : Ara3D.Geometry.EmptyTriMesh;

        public static IGeometry TransformedGeometry(this ISceneNode node)
            => node.Geometry().Transform(node.Transform);

        public static IArray<IGeometry> TransformedGeometries(this IScene scene)
            => scene.Nodes.Select(TransformedGeometry);

        public static IGeometry ToIGeometry(this IScene scene)
            => scene.TransformedGeometries().Merge();

        public static IArray<ISceneNode> Children(this ISceneNode node)
            => node.Scene.ChildNodes(node.Index);

        public static ISceneNode Parent(this ISceneNode node)
            => node.ParentIndex >= 0 ? node.Scene.Nodes[node.ParentIndex] : null;

        public static ISceneNode Root(this IScene scene)
            => scene.Nodes.Count >= 0 ? scene.Nodes[0] : null;
    }
}

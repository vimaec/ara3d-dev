using System.Collections.Generic;
using System.Numerics;

namespace Ara3D
{    
    public class SceneBuilder : IScene
    {
        public SceneNode AddNode(string name, SceneNode parent, Matrix4x4 localTransform, int materialId, IGeometry g = null)
        {
            var sn = new SceneNode
            {
                Name = name,
                Id = NodeList.Count,
                GeometryId = g == null ? -1 : GeometrySet.Add(g),
                ParentId = parent?.Id ?? -1,
                Transform = parent != null ? parent.Transform * localTransform : localTransform,
                MaterialId = materialId,
            };
            NodeList.Add(sn);
            return sn;
        }

        public IndexedSet<IGeometry> GeometrySet;
        public List<ISceneObject> NodeList;

        public IArray<IGeometry> Geometries => GeometrySet.Keys.ToIArray();
        public IArray<ISceneObject> Objects => NodeList.ToIArray();
    }

    public class SceneNode : ISceneObject
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int ParentId { get; set; }
        public Matrix4x4 Transform { get; set; }
        public int GeometryId { get; set; }
        public int MaterialId { get; set; }
    }
}

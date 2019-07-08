using UnityEngine;

namespace Ara3D.UnityBridge
{
    /// <summary>
    /// Manipulate the Buffer mesh to your heart's content, and call "UpdateTarget" whenever you want. 
    /// </summary>
    public class ProceduralMesh
    {
        public MeshClone Original { get; private set; }
        public MeshClone Buffer { get; private set; }
        public Mesh Target { get; private set; }

        public IGeometry OrginalGeometry;
        public IGeometry NewGeometry;

        public ProceduralMesh(Mesh mesh)
        {
            Target = mesh;
            Original = new MeshClone(Target);
            Buffer = Original.Clone();  
        }

        public void ResetTarget()
        {
            Buffer.CopyFrom(Original);
            UpdateTarget();
        }

        public void UpdateTarget()
        {
            Buffer.AssignToMesh(Target);
        }
    }
}

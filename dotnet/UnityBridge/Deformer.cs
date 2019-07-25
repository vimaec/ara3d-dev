using System;
using UnityEngine;
using Ara3D.UnityBridge;

namespace Ara3D
{
    /// <summary>
    /// A Deformer will make a copy of a mesh, and update it according to the deformation function. 
    /// </summary>
    public abstract class Deformer : MonoBehaviour
    {
        private MeshClone _sourceMeshCopy;

        private readonly MemoizedFunction<MeshClone, IGeometry>
            MeshCloneToIGeometry = new MemoizedFunction<MeshClone, IGeometry>((mc) => mc.ToIGeometry());

        public IGeometry SourceGeometry => 
            _sourceMeshCopy != null ? MeshCloneToIGeometry.Call(_sourceMeshCopy) : Geometry.EmptyTriMesh;

        public IGeometry CachedOutputGeometry;

        public abstract IGeometry Deform(IGeometry g);

        public virtual void Update()
        {
            //Debug.Log("Update called");
            var mesh = this.GetMesh();
            if (mesh == null) return;
            _sourceMeshCopy = _sourceMeshCopy ?? (_sourceMeshCopy = new MeshClone(mesh));

            if (_sourceMeshCopy != null)
            {
                var g = Deform(SourceGeometry);

                // TODO: support different geometry types
                if (g.PointsPerFace != 3)
                    throw new Exception("Only triangle meshes supported for now");

                if (g == CachedOutputGeometry)
                    return;

                CachedOutputGeometry = g;

                // TODO: support more types
                mesh.Clear();
                mesh.vertices = g.Vertices.ToUnity();
                mesh.triangles = g.Indices.ToUnityIndexBuffer();
                mesh.RecalculateNormals();
            }
        }

        public virtual void Reset()
        {
            //Debug.Log("Reset");
            Disable();
        }

        public void Enable()
        {
            //Debug.Log("Enabling");
        }

        public void Disable()
        {
            //Debug.Log("Disabling");
            // Restore the original mesh
            _sourceMeshCopy?.AssignToMesh(this.GetMesh());
            _sourceMeshCopy = null;
        }

        public virtual void OnDisable()
        {
            Disable();
        }

        public virtual void OnDestroy()
        {
            Disable();
        }
    }
}

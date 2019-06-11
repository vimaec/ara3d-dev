using Ara3D;
using UnityEngine;

namespace Ara3D
{
    public abstract class ProceduralGeometry : MonoBehaviour
    {
        public abstract IGeometry Geometry { get; }

        public void Reset()
        {
            Start();
        }

        public void Update()
        {
            this.UpdateMesh(Geometry);
        }

        public void OnValidate()
        {
            Update();
        }

        public void Start()
        {
            this.CreateMesh();
            Update();
        }
    }
}

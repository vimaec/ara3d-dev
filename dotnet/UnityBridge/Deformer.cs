using System;
using UnityEngine;
using System.Linq;
using Ara3D.UnityBridge;

namespace Ara3D
{
    /// <summary>
    /// A Deformer will make a copy of a mesh, and update it according to the deformation function. 
    /// </summary>
    public abstract class Deformer : MonoBehaviour
    {
        [NonSerialized] public ProceduralMesh ProcMesh;

        public abstract IGeometry Deform(IGeometry geometry);

        public virtual void Update()
        {
            //Debug.Log("Update called");
            if (ProcMesh != null)
            {
                //Debug.Log("Updating mesh");
                for (var i = 0; i < ProcMesh.Original.Vertices.Length; ++i)
                    ProcMesh.Buffer.Vertices[i] = Deform(ProcMesh.Original.Vertices[i], i);
                ProcMesh.UpdateTarget();
            }
            else
            {
                //Debug.Log("No ProcMesh present");
            }
        }

        public virtual void Start()
        {
            //Debug.Log("Started");
            Update();
        }

        public virtual void Awake()
        {
            //Debug.Log("Awaken");
            Enable();
        }

        public virtual void Reset()
        {
            //Debug.Log("Reset");
            Disable();
            Enable();
        }

        public void Enable()
        {
            //Debug.Log("Enabling");
            if (ProcMesh != null)
                return;
            var mesh = this.GetMesh();
            if (mesh != null)
                ProcMesh = new ProceduralMesh(mesh);
        }

        public void Disable()
        {
            //Debug.Log("Disabling");
            if (ProcMesh != null)
            {
                ProcMesh.ResetTarget();
                ProcMesh = null;
            }
        }

        public virtual void OnEnable()
        {
            //Debug.Log("OnEnable");
            Enable();
        }

        public virtual void OnDisable()
        {
            //Debug.Log("OnDisable");
            Disable();
        }

        public virtual void OnDestroy()
        {
            //Debug.Log("OnDestroy");
            Disable();
        }
    }
}

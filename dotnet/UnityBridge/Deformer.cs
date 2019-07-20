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
        [NonSerialized] public ProceduralMesh ProcMesh;
        [NonSerialized] public IGeometry SourceGeometry;
        [NonSerialized] public IGeometry NewGeometry;

        public abstract IGeometry Deform(IGeometry g);

        public virtual void Update()
        {
            //Debug.Log("Update called");
            if (ProcMesh != null)
            {
                SourceGeometry = ProcMesh.Original.ToGeometry();
                NewGeometry = Deform(SourceGeometry);
                ProcMesh.Buffer.FromGeometry(NewGeometry);
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

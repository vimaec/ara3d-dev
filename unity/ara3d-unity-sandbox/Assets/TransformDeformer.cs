using UnityEngine;
using System;

namespace Ara3D
{
    public enum Axis { 
        XAxis, YAxis, ZAxis
    }

    [ExecuteAlways]
    public class TransformDeformer : Ara3D.Deformer
    {
        public Axis Axis;
        public TransformProperty Transform;        

        public override void Reset()
        {
            Transform = new TransformProperty();            
            base.Reset();
        }

        public override IGeometry Deform(IGeometry g)
        {
            var box = g.BoundingBox();
            return g.Deform(v => v.Transform(Transform.Matrix), v => box.Relative(v).GetComponent((int)Axis));
        }
    }
}
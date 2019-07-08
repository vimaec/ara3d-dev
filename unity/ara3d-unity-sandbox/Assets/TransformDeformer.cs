using UnityEngine;
using System;

namespace Ara3D
{
    [ExecuteAlways]
    public class TransformDeformer : Ara3D.Deformer
    {
        public TransformProperty Transform;

        public override void Reset()
        {
            Transform = new TransformProperty();
            base.Reset();
        }

        public override IGeometry Deform(IGeometry g)
            => g.Deform(v => v.Transform(Transform.Matrix));
    }
}
using UnityEngine;
using Ara3D;

[ExecuteAlways]
public class ModifyGeometry : Ara3D.Deformer
{
    [Range(-100, 100)] public float xOffset;
    [Range(-100, 100)] public float yOffset;
    [Range(-100, 100)] public float zOffset;

    [Range(-10, 10)] public float xScale = 1;
    [Range(-10, 10)] public float yScale = 1;
    [Range(-10, 10)] public float zScale = 1;

    [Range(-180, 180)] public float xRotation;
    [Range(-180, 180)] public float yRotation;
    [Range(-180, 180)] public float zRotation;

    public UnityEngine.Matrix4x4 Matrix { get; private set; } = UnityEngine.Matrix4x4.identity;

    override public void Update()
    {
        ComputeMatrix();
        base.Update();
    }

    public void ComputeMatrix()
    {
        var trans = new UnityEngine.Vector3(xOffset, yOffset, zOffset);
        var scale = new UnityEngine.Vector3(xScale, yScale, zScale);
        var rot = UnityEngine.Quaternion.Euler(xRotation, yRotation, zRotation);
        Matrix = UnityEngine.Matrix4x4.TRS(trans, rot, scale);
    }

    public override void Reset()
    {
        xOffset = 0;
        yOffset = 0;
        zOffset = 0;
        xScale = 1;
        yScale = 1;
        zScale = 1;
        xRotation = 0;
        yRotation = 0;
        zRotation = 0;
        base.Reset();
    }

    public override UnityEngine.Vector3 Deform(UnityEngine.Vector3 v, int i)
        => Matrix.MultiplyPoint(v);
}
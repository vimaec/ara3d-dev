using UnityEngine;
using System;


[Serializable]
public class Translation
{
    [Range(-100, 100)] public float X;
    [Range(-100, 100)] public float Y;
    [Range(-100, 100)] public float Z;

    public Vector3 Vector => new Vector3(X, Y, Z);
}

[Serializable]
public class Scale
{
    [Range(-10, 10)] public float X = 1;
    [Range(-10, 10)] public float Y = 1;
    [Range(-10, 10)] public float Z = 1;

    public Vector3 Vector => new Vector3(X, Y, Z);
}

[Serializable]
public class Rotation
{
    [Range(-180, 180)] public float X;
    [Range(-180, 180)] public float Y;
    [Range(-180, 180)] public float Z;

    public Vector3 Vector => new Vector3(X, Y, Z);
    public Quaternion Quaternion => Quaternion.Euler(Vector);
}

[ExecuteAlways]
public class TransformDeformer : Ara3D.Deformer
{
    public Translation Translation;
    public Scale Scale;
    public Rotation Rotation;

    public Matrix4x4 Matrix { get; private set; } = Matrix4x4.identity;

    override public void Update()
    {
        ComputeMatrix();
        base.Update();
    }

    public void ComputeMatrix()
    {
        Matrix = Matrix4x4.TRS(Translation.Vector, Rotation.Quaternion, Scale.Vector);
    }

    public override void Reset()
    {
        Translation = new Translation();
        Scale = new Scale();
        Rotation = new Rotation();
        base.Reset();
    }

    public override Vector3 Deform(Vector3 v, int i)
        => Matrix.MultiplyPoint(v);
}
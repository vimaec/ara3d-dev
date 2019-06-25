using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Ara3D
{
    [Serializable]
    public class ScaleProperty
    {
        [Range(-10, 10)] public float X = 1;
        [Range(-10, 10)] public float Y = 1;
        [Range(-10, 10)] public float Z = 1;

        public float Multiplier = 1;

        public Vector3 Vector => new Vector3(X, Y, Z) * Multiplier;
    }

    [Serializable]
    public class RotationProperty
    {
        [Range(-180, 180)] public float X;
        [Range(-180, 180)] public float Y;
        [Range(-180, 180)] public float Z;

        public float Multiplier = 1;

        public Vector3 Vector => new Vector3(X, Y, Z) * Multiplier;
        public Quaternion Quaternion => Quaternion.CreateFromYawPitchRoll(X * Multiplier, Y * Multiplier, Z * Multiplier);
    }

    [Serializable]
    public class TranslationProperty
    {
        [Range(-100, 100)] public float X;
        [Range(-100, 100)] public float Y;
        [Range(-100, 100)] public float Z;

        public float Multiplier = 1;

        public Vector3 Vector => new Vector3(X, Y, Z) * Multiplier;
    }

    [CustomPropertyDrawer(typeof(ScaleProperty))]
    public class ScaleDrawerUIE : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create property container element.
            var container = new VisualElement();

            // Create property fields.
            container.Add(new PropertyField(property.FindPropertyRelative("X")));
            container.Add(new PropertyField(property.FindPropertyRelative("Y")));
            container.Add(new PropertyField(property.FindPropertyRelative("Z")));

            return container;
        }
    }

    [CustomPropertyDrawer(typeof(RotationProperty))]
    public class RotationDrawerUIE : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create property container element.
            var container = new VisualElement();

            // Create property fields.
            container.Add(new PropertyField(property.FindPropertyRelative("X")));
            container.Add(new PropertyField(property.FindPropertyRelative("Y")));
            container.Add(new PropertyField(property.FindPropertyRelative("Z")));

            return container;
        }
    }

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

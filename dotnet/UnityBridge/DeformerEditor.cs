using UnityEngine;
using UnityEditor;
using System;

namespace Ara3D
{
    public class DeformableAttribute : Attribute
    {
    }

    [CustomEditor(typeof(DeformableAttribute))]
    public class customButton : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Hi there"))
            {
                this.ResetTarget();
            }
        }

    }
}

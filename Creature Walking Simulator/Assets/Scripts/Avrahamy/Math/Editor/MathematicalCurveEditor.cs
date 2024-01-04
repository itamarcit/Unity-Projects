using UnityEditor;
using UnityEngine;

namespace Avrahamy.Math {
    [CustomPropertyDrawer(typeof(MathematicalCurve))]
    public class MathematicalCurveEditor : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.PropertyField(position, property.FindPropertyRelative("curve"), label);
        }
    }
}
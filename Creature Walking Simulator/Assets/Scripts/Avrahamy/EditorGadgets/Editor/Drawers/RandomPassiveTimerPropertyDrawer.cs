using UnityEditor;
using UnityEngine;
using Avrahamy.EditorGadgets;

namespace Avrahamy {
    [CustomPropertyDrawer(typeof(RandomPassiveTimer), true)]
    public class RandomPassiveTimerPropertyDrawer : PropertyDrawer {
        private const int PADDING = 2;

        private static GUIContent iconNotSet;
        private static GUIContent iconActive;
        private static GUIContent iconNotActive;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            iconNotSet ??= EditorGUIUtility.IconContent("UnityEditor.AnimationWindow");
            iconActive ??= EditorGUIUtility.IconContent("UnityEditor.ProfilerWindow");
            iconNotActive ??= EditorGUIUtility.IconContent("d_ol_minus_act");

            var durationMinProperty = property.FindPropertyRelative("duration.min");
            var durationMaxProperty = property.FindPropertyRelative("duration.max");

            var timer = property.GetValue() as RandomPassiveTimer;

            var icon = iconNotSet;
            if (Application.isPlaying) {
                var endTime = timer.EndTime;
                if (endTime > 0f) {
                    icon = Time.time < endTime ? iconActive : iconNotActive;
                }
            }

            var size = position.height;
            var rect = new Rect(position.x + position.width - size, position.y, size, size);
            GUI.Label(rect, icon);

            if (durationMinProperty.floatValue < 0f) {
                durationMinProperty.floatValue = 0f;
            }
            if (durationMaxProperty.floatValue < durationMinProperty.floatValue) {
                durationMaxProperty.floatValue = durationMinProperty.floatValue;
            }

            position.width -= size + PADDING;
            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            var fieldWidth = (position.width - 10f) / 2;

            EditorGUIUtility.labelWidth = 28f;
            position.width = fieldWidth;
            EditorGUI.PropertyField(position, durationMinProperty, new GUIContent("Min"));

            position.x += 10f + position.width;
            EditorGUI.PropertyField(position, durationMaxProperty, new GUIContent("Max"));

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}
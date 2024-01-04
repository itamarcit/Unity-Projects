using UnityEngine;
using UnityEditor;

namespace Avrahamy.EditorGadgets {
    public class ValueWithChancePropertyDrawer : PropertyDrawer {
        private const int PADDING = 2;
        private const int CHANCE_WIDTH = 30;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var waveProperty = property.FindPropertyRelative("value");
            var chanceProperty = property.FindPropertyRelative("chance");

            position.width -= CHANCE_WIDTH + PADDING;
            EditorGUI.PropertyField(position, waveProperty, label);
            position.x += position.width + PADDING;
            position.width = CHANCE_WIDTH;
            EditorGUI.PropertyField(position, chanceProperty, GUIContent.none);
        }
    }
}

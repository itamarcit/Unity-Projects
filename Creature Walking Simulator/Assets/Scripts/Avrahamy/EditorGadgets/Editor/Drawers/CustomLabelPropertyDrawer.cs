using UnityEditor;
using UnityEngine;

namespace Avrahamy.EditorGadgets {
    public class CustomLabelPropertyDrawer : CompoundPropertyDrawerBase<CustomLabelAttribute>,
            ICompoundAttributeLabelBuilder {
        public GUIContent BuildLabel(ref Rect position, GUIContent label, SerializedProperty property) {
            label.text = attribute.labelText;
            return label;
        }
    }
}
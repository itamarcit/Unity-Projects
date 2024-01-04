using UnityEditor;
using UnityEngine;

namespace Avrahamy.EditorGadgets {
    public class HideIfAssignedPropertyDrawer : CompoundPropertyDrawerBase<HideIfAssignedAttribute>,
            ICompoundAttributeToggle,
            ICompoundAttributeModifier,
            ICompoundAttributeHeightModifier {
        private bool wasEnabled;

        public bool ShouldDraw(SerializedProperty property) {
            switch (property.propertyType) {
                case SerializedPropertyType.ObjectReference:
                    return property.objectReferenceValue == null;
                default:
                    Debug.LogError($"Data type of the property {property.name} used for hide if assigned [{property.propertyType}] is currently not supported");
                    return true;
            }
        }

        public void BeginModifier(SerializedProperty property) {
            wasEnabled = GUI.enabled;
            GUI.enabled = wasEnabled && property.objectReferenceValue == null;
        }

        public void EndModifier() {
            GUI.enabled = wasEnabled;
        }

        public bool GetPropertyHeight(SerializedProperty property, GUIContent label, ref float height, bool wasForced) {
            if (property.objectReferenceValue == null) {
                return false;
            }

            // The property is not being drawn
            // We want to undo the spacing added before and after the property
            height = -EditorGUIUtility.standardVerticalSpacing;
            return true;
        }
    }
}
using UnityEngine;
using UnityEditor;

namespace Avrahamy.EditorGadgets {
    public class HierarchyOnlyPropertyDrawer : CompoundPropertyDrawerBase<HierarchyOnlyAttribute>,
            ICompoundAttributeModifier {
        public void BeginModifier(SerializedProperty property) {
            if (property.objectReferenceValue == null) return;

            // If assigned a game object, try to find a component that implements
            // all of the interfaces and use that as the value of the field.
            var go = property.objectReferenceValue as GameObject;
            if (go == null) {
                var component = property.objectReferenceValue as Component;
                if (component != null) {
                    go = component.gameObject;
                }
            }

            if (go != null) {
                // Game Object is in a scene. It is not a prefab.
                if (go.scene.IsValid()) return;
                var currentGO = property.serializedObject.targetObject as GameObject;
                if (currentGO == null) {
                    var component = property.serializedObject.targetObject as Component;
                    if (component != null) {
                        currentGO = component.gameObject;
                    }
                }
                // Game Object is in the same hierarchy of the Game Object that
                // contains the property. It is OK.
                if (currentGO != null && currentGO.transform.root == go.transform.root) return;
            }

            Debug.LogError($"ERROR: Field {property.name} only accepts things in the hierarchy!");
            property.objectReferenceValue = null;
        }

        public void EndModifier() {
            // Do nothing.
        }
    }
}

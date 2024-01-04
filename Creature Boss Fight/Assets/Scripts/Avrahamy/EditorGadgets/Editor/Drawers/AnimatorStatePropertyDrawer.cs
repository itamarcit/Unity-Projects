using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Avrahamy.EditorGadgets {
    public class AnimatorStatePropertyDrawer
        : CompoundPropertyDrawerBase<AnimatorStateAttribute>,
            ICompoundAttributeView,
            ICompoundAttributeLabelBuilder {
        private Animator animatorComponent;

        public GUIContent BuildLabel(ref Rect position, GUIContent label, SerializedProperty property) {
            if (animatorComponent != null) return label;
            if (property.type != nameof(AnimatorState)) {
                EditorGUI.LabelField(position, "ERROR:", $"AnimatorState may only apply to AnimatorState fields");
                return null;
            }

            // Find the animator by the given field name.
            var animatorProperty = property.FindBaseOrSiblingProperty(attribute.animatorFieldName);
            if (animatorProperty == null) {
                var animatorContainerProperty = property.serializedObject.FindProperty("animator");
                if (animatorContainerProperty != null) {
                    animatorProperty = animatorContainerProperty.FindPropertyRelative(attribute.animatorFieldName);
                }
            }
            if (animatorProperty != null) {
                if (animatorProperty.objectReferenceValue is Animator) {
                    animatorComponent = animatorProperty.objectReferenceValue as Animator;
                } else if (animatorProperty.objectReferenceValue != null) {
                    EditorGUI.LabelField(position, "ERROR:", $"AnimatorStateAttribute referencing field '{attribute.animatorFieldName}' that is not Animator");
                    return null;
                }
            }

            if (animatorComponent == null) {
                EditorGUI.LabelField(position, "ERROR:", $"No Animator found for AnimatorStateAttribute in field '{attribute.animatorFieldName}'");
                return null;
            }

            return label;
        }

        public bool Draw(ref Rect position, Rect originalRect, SerializedProperty property, GUIContent label) {
            if (animatorComponent == null) return true;

            if (animatorComponent.GetCurrentAnimatorStateInfo(0).length == 0) {
                animatorComponent.Rebind();
            }

            if (label != null && property.hasChildren) {
                position = EditorGUI.PrefixLabel(position, label);
            }

            var layerNameProperty = property.FindPropertyRelative("layer");
            var layerIndexProperty = property.FindPropertyRelative("layerIndex");
            var stateNameProperty = property.FindPropertyRelative("name");

            var layerName = layerNameProperty.stringValue;

            // Get the list of layer names in animator.
            var layerNames = new List<string>();
            var layerCount = animatorComponent.layerCount;
            for (int i = 0; i < layerCount; i++) {
                var item = animatorComponent.GetLayerName(i);
                layerNames.Add(item);

                if (item == layerName) {
                    layerIndexProperty.intValue = i;
                }
            }

            // Make sure the layer is defaulted to the base layer.
            if (string.IsNullOrEmpty(layerName)) {
                layerName = layerNames[0];
                layerIndexProperty.intValue = 0;
                layerNameProperty.stringValue = layerName;
                property.serializedObject.ApplyModifiedProperties();
            }

            // Get the list of clip names in animator.
            var clips = animatorComponent.runtimeAnimatorController.animationClips;
            var clipNames = new List<string>();
            foreach (var animatorParam in clips) {
                clipNames.Add(animatorParam.name);
            }

            const float HALF_SPACE = 5f;
            position.width = position.width * 0.5f - HALF_SPACE;
            InspectorUtilities.ShowDropdownForStringProperty(position, layerNameProperty, layerNames, false);
            position.x += position.width + HALF_SPACE + HALF_SPACE;
            InspectorUtilities.ShowDropdownForStringProperty(position, stateNameProperty, clipNames);
            return true;
        }
    }
}

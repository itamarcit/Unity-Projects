using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using Object = UnityEngine.Object;

namespace Avrahamy.EditorGadgets {
    public class StringListDropdownPropertyDrawer :
            CompoundPropertyDrawerBase<StringListDropdownAttribute>,
            ICompoundAttributeLabelBuilder,
            ICompoundAttributeView,
            ICompoundAttributeHeightModifier {
        private const float X_POSITION_MARGIN = 2f;
        private const float Y_POSITION_MARGIN = 5f;
        private const float VALUE_SELECTION_HEIGHT = 18f;
        private const float HELP_BOX_HEIGHT = 35f;
        private const float CREATE_NEW_BTN_HEIGHT = 20f;
        private const float SELECT_BTN_WIDTH = 20f;

        private IStringList stringListAsset;

        public GUIContent BuildLabel(ref Rect position, GUIContent label, SerializedProperty property) {
            if (property.propertyType != SerializedPropertyType.String) {
                EditorGUI.LabelField(position, "ERROR:", "May only apply to type string");
                return null;
            }
            return EditorGUI.BeginProperty(position, label, property);
        }

        public bool Draw(ref Rect position, Rect originalRect, SerializedProperty property, GUIContent label) {
            if (label != null && property.hasChildren) {
                position = EditorGUI.PrefixLabel(position, label);
            }

            var stringListAssetName = attribute.stringListAssetName;

            position.height = VALUE_SELECTION_HEIGHT;

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var stringValues = GetStringListAssetValues(stringListAssetName);
            if (stringValues != null) {
                position.width -= SELECT_BTN_WIDTH + X_POSITION_MARGIN;
                InspectorUtilities.ShowDropdownForStringProperty(position, property, stringValues);
                position.x += position.width + X_POSITION_MARGIN;
                position.width = SELECT_BTN_WIDTH;
                if (GUI.Button(position, string.Empty)) {
                    Selection.activeObject = stringListAsset as Object;
                }
            } else {
                EditorGUI.TextField(position, property.stringValue);

                position = new Rect(originalRect.x, GetYPosWithMargin(position), originalRect.width, HELP_BOX_HEIGHT);
                EditorGUI.HelpBox(position, $"No StringList asset named {stringListAssetName} found", MessageType.Warning);

                position = new Rect(originalRect.x, GetYPosWithMargin(position), originalRect.width, CREATE_NEW_BTN_HEIGHT);
                if (GUI.Button(position, $"Create {stringListAssetName}")) {
                    var path = EditorUtility.SaveFolderPanel(
                        $"Select folder to save '{stringListAssetName}.asset'",
                        Path.Combine(Application.dataPath, "Avrahamy/Configs"),
                        string.Empty);
                    EditorUtils.CreateScriptableObject<StringList>(path, stringListAssetName);
                }
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
            return true;
        }

        public bool GetPropertyHeight(SerializedProperty property, GUIContent label, ref float height, bool wasForced) {
            var stringValues = GetStringListAssetValues(attribute.stringListAssetName);

            if (stringValues != null) return wasForced;
            height += HELP_BOX_HEIGHT + CREATE_NEW_BTN_HEIGHT + Y_POSITION_MARGIN * 3;
            return true;
        }

        private string[] GetStringListAssetValues(string stringListAssetName) {
            if (stringListAsset != null) return stringListAsset.Values ?? Array.Empty<string>();

            var allStringListAssets = EditorUtils.FindAllAssetsOfType<ScriptableObject>();
            foreach (var asset in allStringListAssets) {
                if (!(asset is IStringList stringList)) continue;
                if (asset.name != stringListAssetName) continue;
                stringListAsset = stringList;
                return stringListAsset.Values ?? Array.Empty<string>();
            }
            return null;
        }

        private static float GetYPosWithMargin(Rect currentPos) {
            return currentPos.y + currentPos.height + Y_POSITION_MARGIN;
        }
    }
}

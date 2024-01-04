using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace Avrahamy {
    public static class EditorExtensions {
        private const string INFRA_NAMESPACE = "Avrahamy";

        [MenuItem("Tools/GG/Toggle Developer Mode")]
        private static void ToggleDeveloperMode() {
            EditorPrefs.SetBool("DeveloperMode", !EditorPrefs.GetBool("DeveloperMode", false));
        }

        [MenuItem("Tools/GG/Log Far Objects")]
        public static void FindFarObjects() {
            var farObjs = new List<GameObject>();
            var allObjs = Object.FindObjectsOfType<GameObject>();
            foreach (var t in allObjs) {
                if (Mathf.Abs(t.transform.position.x) > 1000
                    || Mathf.Abs(t.transform.position.y) > 1000
                    || Mathf.Abs(t.transform.position.z) > 1000){
                    farObjs.Add(t);
                }
            }

            if (farObjs.Count > 0) {
                foreach (var t in farObjs) {
                    Debug.LogError($"Found object {t.name} at location {t.transform.position}", t);
                }
            } else {
                Debug.Log("No Far objects");
            }
        }

        [MenuItem("Assets/Editor/Create Editor Script", true)]
        private static bool CreateEditorScriptValidation() {
            return Selection.activeObject is MonoScript;
        }

        [MenuItem("Assets/Editor/Create Editor Script")]
        private static void CreateEditorScript() {
            const string text = @"using UnityEditor;
using UnityEngine;

namespace {0} {{
    [CustomEditor(typeof({1}))]
    public class {2}Editor : Editor {{
        public override void OnInspectorGUI() {{
            DrawDefaultInspector();

            // TODO: Implement.
        }}
    }}
}}";
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var className = Path.GetFileNameWithoutExtension(path);
            var namespaceName = path.Contains("Scripts/" + INFRA_NAMESPACE + "/") ? INFRA_NAMESPACE : "Product";
            var directoryName = Path.GetDirectoryName(path);
            var editorDirectory = path.Contains("/Editor/") ? directoryName : Path.Combine(directoryName, "Editor");
            path = Path.Combine(editorDirectory, className + "Editor.cs");
            Directory.CreateDirectory(editorDirectory);
            File.WriteAllText(path, string.Format(text, namespaceName, className, className));
            AssetDatabase.Refresh();

            Debug.LogError("Created " + path);
        }
    }
}

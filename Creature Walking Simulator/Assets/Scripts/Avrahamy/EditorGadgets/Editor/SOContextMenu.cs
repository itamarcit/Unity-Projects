using UnityEditor;
using UnityEngine;
using System.IO;
using Avrahamy.Utils;

namespace Avrahamy.EditorGadgets {
    public static class SOContextMenu {
        private const string SCRIPT_GUID_PREFIX = "m_Script: {fileID: 11500000, guid: ";
        private const string SCRIPT_GUID_SUFFIX = ", type:";
        private const string META_GUID_PREFIX = "guid: ";

        [MenuItem ("Assets/Scriptable Object/Swap Class"), MenuItem("CONTEXT/ScriptableObject/Swap Class")]
        public static void SwapClass(MenuCommand cmd) {
            var so = cmd.context as ScriptableObject ?? Selection.activeObject as ScriptableObject;
            if (so == null) return;
            var path = AssetDatabase.GetAssetPath(so);
            DebugLog.LogError("Path " + path);
            var data = File.ReadAllText(path);
            DebugLog.LogError(data);
            var guid = data.Substring(SCRIPT_GUID_PREFIX, SCRIPT_GUID_SUFFIX);
            if (guid == null) return;
            DebugLog.LogError("GUID: " + guid);
            var newClassPath = EditorUtility.OpenFilePanelWithFilters(
                "Select SO script to replace with",
                Path.Combine(Application.dataPath, "Scripts/Product"),
                new [] {"CSharp", "cs"});
            if (string.IsNullOrEmpty(newClassPath)) return;
            var metaData = File.ReadAllLines(newClassPath + ".meta")[1];
            DebugLog.LogError(metaData);
            var newGuid = metaData.EndString(META_GUID_PREFIX);
            DebugLog.LogError("New GUID: " + newGuid);
            data = data.Replace(guid, newGuid);
            File.WriteAllText(path, data);
            AssetDatabase.ImportAsset(path);
        }

        // Validate
        [MenuItem ("Assets/Scriptable Object/Swap Class", true)]
        public static bool ValidateSwapClass(MenuCommand cmd) {
            return Selection.activeObject is ScriptableObject;
        }
    }
}

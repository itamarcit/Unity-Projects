using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace Avrahamy.EditorGadgets {
    public static class RemoveEmptyFolders {
        [MenuItem("Tools/Remove empty folders", false, -301)]
        private static void RemoveEmptyFoldersMenuItem() {
            var index = Application.dataPath.IndexOf("/Assets", System.StringComparison.Ordinal);
            var projectSubfolders = Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories);

            // Create a list of all the empty subfolders under Assets.
            var emptyFolders = projectSubfolders.Where(IsEmptyRecursive).ToArray();

            foreach (var folder in emptyFolders) {
                // Verify that the folder exists (may have been already removed).
                if (Directory.Exists(folder)) {
                    Debug.Log("Deleting : " + folder);

                    // Remove dir (recursively)
                    Directory.Delete(folder, true);
                    File.Delete(folder + ".meta");

                    // Sync AssetDatabase with the delete operation.
                    AssetDatabase.DeleteAsset(folder.Substring(index + 1));
                }
            }

            // Refresh the asset database once we're done.
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// A helper method for determining if a folder is empty or not.
        /// </summary>
        private static bool IsEmptyRecursive(string path) {
            // A folder is empty if it (and all its subdirs) have no files (ignore .meta files)
            return !Directory.GetFiles(path).Select(file => !file.EndsWith(".meta", System.StringComparison.Ordinal)).Any()
            && Directory.GetDirectories(path, string.Empty, SearchOption.AllDirectories).All(IsEmptyRecursive);
        }
    }
}

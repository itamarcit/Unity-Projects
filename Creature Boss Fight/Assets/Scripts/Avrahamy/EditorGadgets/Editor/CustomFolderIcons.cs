using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Avrahamy.EditorGadgets {
    [CreateAssetMenu(menuName = "Avrahamy/Setup/Custom Folder Icons", fileName = "AudioModel")]
    [InitializeOnLoad]
    public class CustomFolderIcons : ScriptableObject {
        public enum FolderType {
            Name,
            Path,
        }

        private const float LARGE_ICON_SIZE = 64f;

        [Serializable]
        public class Folder {
            public string key;
            public FolderType keyType;
            public Texture2D smallIcon;
            public Texture2D largeIcon;
            public bool affectSubFolders;
        }

        public static CustomFolderIcons Instance {
            get {
                if (_instance == null) {
                    var assets = EditorUtils.FindAllAssetsOfType<CustomFolderIcons>();
                    if (assets.Length > 0) {
                        _instance = assets[0];
                    } else {
                        EditorUtils.CreateScriptableObject<CustomFolderIcons>(Application.dataPath, "Custom Folder Icons");
                        DebugLog.LogWarning("Created asset 'Custom Folder Icons'");
                    }
                }
                return _instance;
            }
        }

        [SerializeField] List<Folder> folders;

        private static CustomFolderIcons _instance;
        private Texture2D defaultIcon;

        public Texture2D DefaultFolderIcon {
            get {
                if (defaultIcon == null) {
                    defaultIcon = EditorGUIUtility.FindTexture("Folder Icon");
                }

                return defaultIcon;
            }
        }

        static CustomFolderIcons() {
            EditorApplication.projectWindowItemOnGUI += ReplaceFolderIcon;
        }

        private static void ReplaceFolderIcon(string guid, Rect rect) {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!AssetDatabase.IsValidFolder(path)) return;

            var isSmall = IsIconSmall(ref rect);

            var settings = Instance;
            if (settings == null) return;
            var texture = settings.GetFolderIcon(path, isSmall);
            if (texture == null) return;

            DrawCustomIcon(rect, texture, isSmall);
        }

        public Texture2D GetFolderIcon(string folderPath, bool small = true) {
            var folder = GetFolderByPath(folderPath, true);
            if (folder == null) return null;

            return small ? folder.smallIcon : folder.largeIcon;
        }

        public Folder GetFolderByPath(string folderPath, bool allowRecursive = false) {
            if (folders == null || folders.Count == 0) return null;

            for (var index = folders.Count - 1; index >= 0; index--) {
                var folder = folders[index];
                switch (folder.keyType) {
                    case FolderType.Name:
                        var folderName = Path.GetFileName(folderPath);
                        if (allowRecursive && folder.affectSubFolders) {
                            if (folderPath.Contains($"/{folder.key}/")) return folder;
                        } else {
                            if (folder.key.Equals(folderName)) return folder;
                        }
                        break;
                    case FolderType.Path:
                        if (allowRecursive && folder.affectSubFolders) {
                            if (folderPath.StartsWith(folder.key)) return folder;
                        } else {
                            if (folder.key.Equals(folderPath)) return folder;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return null;
        }

        private static void DrawCustomIcon(Rect rect, Texture texture, bool isSmall) {
            /*if (rect.width > LARGE_ICON_SIZE) {
                // center the icon if it is zoomed
                var offset = (rect.width - LARGE_ICON_SIZE) / 2f;
                rect = new Rect(rect.x + offset, rect.y + offset, LARGE_ICON_SIZE, LARGE_ICON_SIZE);
            } else {
                if (isSmall && !IsTreeView(rect)) {
                    rect = new Rect(rect.x + 3, rect.y, rect.width, rect.height);
                }
            }*/

            GUI.DrawTexture(rect, texture);
        }

        private static bool IsIconSmall(ref Rect rect) {
            var isSmall = rect.width > rect.height;

            if (isSmall) {
                rect.width = rect.height;
            } else {
                rect.height = rect.width;
            }

            return isSmall;
        }

        private static bool IsTreeView(Rect rect) {
            return (rect.x - 16) % 14 == 0;
        }

        public static EditorWindow GetProjectWindow() {
            return GetWindowByName("UnityEditor.ProjectWindow")
                   ?? GetWindowByName("UnityEditor.ObjectBrowser")
                   ?? GetWindowByName("UnityEditor.ProjectBrowser");
        }

        private static EditorWindow GetWindowByName(string pName) {
            var objectList = Resources.FindObjectsOfTypeAll(typeof(EditorWindow));
            return (from obj in objectList where obj.GetType().ToString() == pName select (EditorWindow)obj).FirstOrDefault();
        }
    }
}

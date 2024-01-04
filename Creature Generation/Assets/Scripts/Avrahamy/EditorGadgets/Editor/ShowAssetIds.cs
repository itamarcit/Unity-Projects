//Get instance, guid, and file ID of file during play mode
//Possibly useful for getting the type or instance of a specific file in the project folder and possibly loading
//This script prints these details for a selected asset in project window

using UnityEngine;
using UnityEditor;
using System.Text;

namespace Avrahamy.EditorGadgets {
	public class ShowAssetIds {
		[MenuItem("Assets/Show Asset Ids")]
		public static void MenuShowIds() {
			var stringBuilder = new StringBuilder();

			foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(Selection.activeObject))) {
				if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var guid, out long file)) {
					stringBuilder.AppendLine("Asset: " + obj.name + "\n  Instance ID: " + obj.GetInstanceID() + "\n  GUID: " + guid + "\n  File ID: " + file);
					if (stringBuilder.Length >= 16000) {
						Debug.Log(stringBuilder.ToString());
						stringBuilder.Clear();
						stringBuilder.Append("Continued: ");
					}
				}
			}

			Debug.Log(stringBuilder.ToString());
		}
	}
}
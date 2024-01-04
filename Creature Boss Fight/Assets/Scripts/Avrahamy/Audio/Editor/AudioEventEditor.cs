using UnityEngine;
using UnityEditor;

namespace Avrahamy.Audio {
    [CustomEditor(typeof(AudioEvent), true)]
    public class AudioEventEditor : Editor {
        [SerializeField] private AudioSource _previewer;
        private ExtendedAudioSource previewSource;

        protected void OnEnable() {
            _previewer = EditorUtility.CreateGameObjectWithHideFlags("Audio preview", HideFlags.HideAndDontSave, typeof(AudioSource)).GetComponent<AudioSource>();
            previewSource = new ExtendedAudioSource(_previewer);
        }

        protected void OnDisable() {
            DestroyImmediate(_previewer.gameObject);
            previewSource = null;
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            EditorGUI.BeginDisabledGroup(serializedObject.isEditingMultipleObjects);
            if (GUILayout.Button("Preview")) {
                ((AudioEvent)target).Play(previewSource);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}

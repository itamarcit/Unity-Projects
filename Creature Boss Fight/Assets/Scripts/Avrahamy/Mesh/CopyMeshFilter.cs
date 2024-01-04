using UnityEngine;

namespace Avrahamy.Meshes {
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer))]
    public class CopyMeshFilter : MonoBehaviour {
        [SerializeField] MeshFilter referenceMeshFilter;

        private MeshFilter ownMeshFilter;

        protected void Awake() {
            ownMeshFilter = GetComponent<MeshFilter>();
            ownMeshFilter.sharedMesh = referenceMeshFilter.sharedMesh;
        }

        protected void Reset() {
            Awake();
        }

        protected void LateUpdate() {
#if UNITY_EDITOR
            // In edit mode, always copy the referenced mesh.
            if (Application.isPlaying)
#endif
            if (ownMeshFilter.sharedMesh != null) return;
            ownMeshFilter.sharedMesh = referenceMeshFilter.sharedMesh;
        }
    }
}

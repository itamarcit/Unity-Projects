using UnityEngine;

namespace Avrahamy.Meshes {
    /// <summary>
    /// Based on this slide: https://docs.google.com/presentation/d/10XjxscVrm5LprOmG-VB2DltVyQ_QygD26N6XC2iap2A/edit#slide=id.gb871cf6ef_0_0
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    public class UniqueMesh : OptimizedBehaviour {
        // To ensure they have a unique mesh
        [HideInInspector, SerializeField] int ownerID;

        // Tries to find a mesh filter, adds one if it doesn't exist yet
        protected MeshFilter MeshFilter {
            get {
                if (meshFilter == null) {
                    meshFilter = GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
                }
                if (mesh != null && meshFilter.sharedMesh == null) {
                    meshFilter.sharedMesh = mesh;
                }
                return meshFilter;
            }
        }
        private MeshFilter meshFilter;

        public Mesh Mesh {
            get {
                var isOwner = ownerID == name.GetHashCode();
                if (mesh == null || !isOwner) {
                    MeshFilter.sharedMesh = mesh = new Mesh();
                    mesh.hideFlags = HideFlags.DontSave;
                    ownerID = name.GetHashCode();
                    mesh.name = "Mesh [" + ownerID + "]";
                }
                return mesh;
            }
        }
        private Mesh mesh;
    }

}

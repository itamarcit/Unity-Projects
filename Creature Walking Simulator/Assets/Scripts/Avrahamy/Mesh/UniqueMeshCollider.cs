using UnityEngine;

namespace Avrahamy.Meshes {
    /// <summary>
    /// Based on this slide: https://docs.google.com/presentation/d/10XjxscVrm5LprOmG-VB2DltVyQ_QygD26N6XC2iap2A/edit#slide=id.gb871cf6ef_0_0
    /// </summary>
    [RequireComponent(typeof(MeshCollider))]
    public class UniqueMeshCollider : MonoBehaviour {
        // To ensure they have a unique mesh
        [HideInInspector, SerializeField] int ownerID;

        // Tries to find a mesh filter, adds one if it doesn't exist yet
        protected MeshCollider MeshCollider {
            get{
                _mc = _mc == null ? GetComponent<MeshCollider>() : _mc;
                _mc = _mc == null ? gameObject.AddComponent<MeshCollider>() : _mc;
                return _mc;
            }
        }
        private MeshCollider _mc;

        public Mesh Mesh {
            get{
                bool isOwner = ownerID == name.GetHashCode();
                if (_mesh == null || !isOwner) {
                    MeshCollider.sharedMesh = _mesh = new Mesh();
                    ownerID = name.GetHashCode();
                    _mesh.name = "CollisionMesh [" + ownerID + "]";
                } else if (MeshCollider.sharedMesh == null) {
                    MeshCollider.sharedMesh = _mesh;
                }
                return _mesh;
            }
        }
        private Mesh _mesh;
    }

}

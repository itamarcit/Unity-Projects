using UnityEngine;

namespace Flocking {
    public class PeepModel : MonoBehaviour {
        [SerializeField] int group;
        [SerializeField] float selfRadius;

        public int Group {
            get {
                return group;
            }
            set {
                group = value;
            }
        }

        public float SelfRadius {
            get {
                return selfRadius;
            }
        }

        protected void Reset() {
            if (selfRadius > 0f) return;
            var capsule = GetComponentInChildren<CapsuleCollider>();
            if (capsule != null) {
                selfRadius = capsule.radius;
            }
            var box = GetComponentInChildren<BoxCollider>();
            if (box != null) {
                var extents = box.bounds.extents;
                selfRadius = Mathf.Max(extents.x, extents.z);
            }
        }
    }
}

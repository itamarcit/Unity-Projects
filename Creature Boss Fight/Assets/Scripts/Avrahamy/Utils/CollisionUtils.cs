using UnityEngine;

namespace Avrahamy {
    public static class CollisionUtils {
        public static bool Contains(this LayerMask layerMask, int layer) {
            return (layerMask.value & (1 << layer)) > 0;
        }
    }
}

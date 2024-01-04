using UnityEngine;

namespace Avrahamy {
    public class OptimizedUIBehaviour : MonoBehaviour {
        public RectTransform RectTransform {
            get {
                if (rectTransform == null) {
                    rectTransform = GetComponent<RectTransform>();
                }
                return rectTransform;
            }
        }

        private RectTransform rectTransform;
    }
}

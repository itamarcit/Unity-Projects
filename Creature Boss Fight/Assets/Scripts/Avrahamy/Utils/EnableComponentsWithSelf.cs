using UnityEngine;

namespace Avrahamy.Utils {
    public class EnableComponentsWithSelf : MonoBehaviour {
        [SerializeField] Behaviour[] components;
        [SerializeField] bool shouldEnable = true;

        protected void OnEnable() {
            foreach (var component in components) {
                component.enabled = shouldEnable;
            }
        }
    }
}

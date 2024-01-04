using UnityEngine;

namespace Avrahamy.Utils {
    public class EnableOtherWithSelf : MonoBehaviour {
        [SerializeField] bool invert;
        [SerializeField] GameObject[] others;

        protected void OnEnable() {
            foreach (var other in others) {
                other.SetActive(!invert);
            }
        }

        protected void OnDisable() {
            foreach (var other in others) {
                other.SetActive(invert);
            }
        }
    }
}

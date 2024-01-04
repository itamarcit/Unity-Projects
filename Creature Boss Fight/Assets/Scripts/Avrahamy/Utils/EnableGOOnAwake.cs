using UnityEngine;

namespace Avrahamy.Utils {
    public class EnableGOOnAwake : MonoBehaviour {
        [SerializeField] bool shouldEnable = true;
        [SerializeField] GameObject[] gameObjects;

        protected void Awake() {
            foreach (var go in gameObjects) {
                go.SetActive(shouldEnable);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

namespace Avrahamy {
    public class DebugLifecycle : MonoBehaviour, ISelectHandler, IDeselectHandler {
        protected void Awake() {
            DebugLog.Log($"Awake {this}", this);
        }

        protected void Start() {
            DebugLog.Log($"Start {this}", this);
        }

        protected void OnDestroy() {
            DebugLog.Log($"OnDestroy {this}");
        }

        protected void OnEnable() {
            DebugLog.Log($"OnEnable {this}", this);
        }

        protected void OnDisable() {
            DebugLog.Log($"OnDisable {this}", this);
        }

        public void OnSelect(BaseEventData eventData) {
            DebugLog.Log($"OnSelect {this}", this);
        }

        public void OnDeselect(BaseEventData eventData) {
            DebugLog.Log($"OnDeselect {this}", this);
        }
    }
}

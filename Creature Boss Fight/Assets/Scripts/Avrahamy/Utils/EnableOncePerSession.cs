using UnityEngine;
using System.Collections.Generic;
using BitStrap;

namespace Avrahamy.Utils {
    public class EnableOncePerSession : MonoBehaviour {
        [HelpBox("This works by saving the ID in a static list", HelpBoxAttribute.MessageType.Info)]
        [SerializeField] int id;

        private static readonly List<int> enabledIDs = new List<int>();
        private bool wasDisabled = true;

        protected void OnEnable() {
            if (enabledIDs.Contains(id)) {
                if (!wasDisabled) return;
                gameObject.SetActive(false);
                return;
            }
            enabledIDs.Add(id);
        }

        protected void OnDisable() {
            // Only considered as disabled when disabling self and not parent.
            wasDisabled = !gameObject.activeSelf;
        }

#if UNITY_EDITOR
        [Button]
        private void ClearList() {
            enabledIDs.Clear();
        }
        #endif
    }
}

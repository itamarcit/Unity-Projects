using UnityEngine;
using System;
using System.Diagnostics;

namespace Avrahamy.EditorGadgets {
    /// <summary>
    /// Should be used with an [AnimatorState] attribute.
    /// </summary>
    [Serializable]
    public class AnimatorState : AnimatorParameter {
        [SerializeField] string layer;
        [SerializeField] int layerIndex;

        public string Layer {
            get {
                return layer;
            }
        }

        public int LayerIndex {
            get {
                return layerIndex;
            }
        }

        public override bool IsNull {
            get {
                return base.IsNull || string.IsNullOrEmpty(layer);
            }
        }

        public override void CacheHash() {
            hash = Animator.StringToHash($"{layer}.{Name}");
        }

        [Conditional("DEBUG_LOG")]
        public new void AssertExists(Animator animator) {
            if (string.IsNullOrEmpty(Name)) {
                return;
            }
            var clips = animator.runtimeAnimatorController.animationClips;
            foreach (var parameter in clips) {
                if (parameter.name == Name) {
                    return;
                }
            }
            DebugAssert.Assert(false, $"{animator} does not contain a state named {Name}");
        }

        public override void Clear() {
            base.Clear();
            layer = null;
        }
    }
}

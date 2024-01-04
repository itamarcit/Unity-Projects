using UnityEngine;
using System;
using System.Diagnostics;

namespace Avrahamy.EditorGadgets {
    [Serializable]
    public class AnimatorParameter {
        [SerializeField] string name;

        public string Name {
            get {
                return name;
            }
        }

        public int Hash {
            get {
                // By default, only cache hashes in a build.
#if !UNITY_EDITOR || CACHE_ANIMATOR_HASHES
                if (hash == null)
#endif
                    CacheHash();
                return (int)hash;
            }
        }

        public virtual bool IsNull {
            get {
                return string.IsNullOrEmpty(name);
            }
        }

        protected int? hash;

        public virtual void CacheHash() {
            hash = Animator.StringToHash(name);
        }

        public static implicit operator int(AnimatorParameter parameter) {
            return parameter.Hash;
        }

        [Conditional("DEBUG_LOG")]
        public void AssertExists(Animator animator) {
            if (string.IsNullOrEmpty(name)) {
                return;
            }
            foreach (var parameter in animator.parameters) {
                if (parameter.name == name) {
                    return;
                }
            }
            DebugAssert.Assert(false, $"{animator} does not contain parameter named {name}");
        }

        public virtual void Clear() {
            name = null;
        }
    }
}

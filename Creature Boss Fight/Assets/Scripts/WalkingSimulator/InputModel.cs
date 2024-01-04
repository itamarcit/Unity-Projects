using UnityEngine;

namespace WalkingSimulator {
    public class InputModel : MonoBehaviour {
        [SerializeField] bool alwaysLockFacing;
        
        public Vector2 MoveInput {
            get {
                return moveInput;
            }
            set {
                moveInput = value;
            }
        }

        public bool IsLockFacing {
            get {
                return alwaysLockFacing || isLockFacing;
            }
            set {
                isLockFacing = value;
            }
        }

        private Vector2 moveInput;
        private bool isLockFacing;
    }
}

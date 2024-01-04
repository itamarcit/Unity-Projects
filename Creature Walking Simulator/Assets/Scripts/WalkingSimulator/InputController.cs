using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Avrahamy;

namespace ItamarRamon {
    public class InputController : MonoBehaviour, InputActions.IGameplayActions {
        [SerializeField] InputModel inputModel;
        [SerializeField] bool flipX = true;
        [SerializeField] bool flipY;

        private static InputActions actions;
        
        protected void Awake() {
            actions ??= new InputActions();
            actions.Gameplay.Enable();
            actions.Gameplay.SetCallbacks(this);

            inputModel.MoveInput = Vector2.zero;
            inputModel.IsLockFacing = false;
        }

        protected void Reset() {
            if (inputModel == null) {
                inputModel = GetComponent<InputModel>();
            }
        }

        public void OnMove(InputAction.CallbackContext context) {
            Vector2 moveInput;
            try {
                moveInput = context.action.ReadValue<Vector2>();
            } catch (Exception e) {
                DebugLog.LogError(e);
                inputModel.MoveInput = Vector2.zero;
                return;
            }
            if (flipX) {
                moveInput.x = -moveInput.x;
            }
            if (flipY) {
                moveInput.y = -moveInput.y;
            }
            inputModel.MoveInput = moveInput.normalized;
        }

        public void OnLockFacing(InputAction.CallbackContext context) {
            inputModel.IsLockFacing = context.phase == InputActionPhase.Performed;
        }
    }
}

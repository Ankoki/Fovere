using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static Fovere.PlayerInputActions;

namespace Fovere
{
    [CreateAssetMenu(fileName = "New InputReader", menuName = "InputReader")]
    public class PrototypeInputReader : ScriptableObject, IPlayerActions
    {
        private static bool IsDeviceMouse(InputAction.CallbackContext context) => context.control.device.name == "Mouse";
        
        public event UnityAction<Vector2> Move = delegate { };
        public event UnityAction<Vector2, bool> Look = delegate { };
        public event UnityAction Jump = delegate { };
        public event UnityAction Interact = delegate { };
        
        public Vector3 Direction => _inputActions.Player.Move.ReadValue<Vector2>();
        
        private PlayerInputActions _inputActions;

        private void OnEnable()
        {
            if (_inputActions == null)
            {
                _inputActions = new PlayerInputActions();
                _inputActions.Player.SetCallbacks(this);
            }
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions?.Dispose();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            Move?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            Look?.Invoke(context.ReadValue<Vector2>(), IsDeviceMouse(context));
        }


        public void OnAttack(InputAction.CallbackContext context)
        {
            // noop
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            Interact?.Invoke();
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            // noop
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            Jump?.Invoke();
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            // noop
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            // noop
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            // noop
        }
        
    }
}
using UnityEngine;

namespace KadaXuanwu.Utils.Runtime.FPController.InputAbstraction.Example {
    /// <summary>
    /// Default implementation using Unity's Input System via InputManager.
    /// </summary>
    public class InputSystemCharacterInput : ICharacterInput {
        private InputSystemControls.PlayerActions _actions;

        public Vector2 MoveInput => _actions.Move.ReadValue<Vector2>();
        public Vector2 LookInput => _actions.Look.ReadValue<Vector2>();
        public bool JumpPressed => _actions.Jump.WasPressedThisFrame();
        public bool JumpHeld => _actions.Jump.IsPressed();
        public bool SprintPressed => _actions.Sprint.IsPressed();
        public bool CrouchPressed => _actions.Crouch.IsPressed();

        public void Initialize(InputSystemControls.PlayerActions actions) {
            _actions = actions;
        }
    }
}

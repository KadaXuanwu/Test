using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KadaXuanwu.Utils.Runtime.Input {
    /// <summary>
    /// Centralized input manager that wraps the generated PlayerInputActions class.
    /// Provides unified control over all input (enable/disable, action maps, etc.)
    /// 
    /// SETUP:
    /// 1. Create Input Actions asset with your bindings
    /// 2. Check "Generate C# Class" in the asset inspector → Apply
    /// 3. Rename the generated class to "InputSystemControls" (or update the reference here)
    /// 4. Add InputManager to a persistent GameObject (or use Instance directly)
    /// 
    /// USAGE:
    ///     // Polling
    ///     Vector2 move = InputManager.Instance.Actions.Player.Move.ReadValue&lt;Vector2&gt;();
    ///     bool isJumping = InputManager.Instance.Actions.Player.Jump.IsPressed();
    ///     
    ///     // Events
    ///     InputManager.Instance.Actions.Player.Jump.performed += OnJump;
    ///     
    ///     // Disable all input (e.g., during pause/cutscene)
    ///     InputManager.Instance.DisableAllInput();
    ///     InputManager.Instance.EnableAllInput();
    ///     
    ///     // Switch action maps
    ///     InputManager.Instance.SwitchToActionMap(InputManager.Instance.Actions.UI);
    /// </summary>
    public class InputManager : MonoBehaviour {
        public static InputManager Instance { get; private set; }

        public InputSystemControls Actions { get; private set; }
        public bool InputEnabled { get; private set; } = true;

        public event Action OnInputEnabled;
        public event Action OnInputDisabled;

        private InputActionMap _activeActionMap;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Actions = new InputSystemControls();
            _activeActionMap = Actions.Player.Get();
        }

        private void OnEnable() {
            EnableAllInput();
        }

        private void OnDisable() {
            DisableAllInput();
        }

        private void OnDestroy() {
            if (Instance == this) {
                Actions?.Dispose();
                Instance = null;
            }
        }

        public void EnableAllInput() {
            Actions.Enable();
            InputEnabled = true;
            OnInputEnabled?.Invoke();
        }

        public void DisableAllInput() {
            Actions.Disable();
            InputEnabled = false;
            OnInputDisabled?.Invoke();
        }

        public void SwitchToActionMap(InputActionMap actionMap) {
            if (_activeActionMap == actionMap) {
                return;
            }

            _activeActionMap?.Disable();
            _activeActionMap = actionMap;

            if (InputEnabled) {
                _activeActionMap?.Enable();
            }
        }

        public void EnableActionMap(InputActionMap actionMap) {
            if (InputEnabled) {
                actionMap?.Enable();
            }
        }

        public void DisableActionMap(InputActionMap actionMap) {
            actionMap?.Disable();
        }

        /// <summary>
        /// Temporarily disables input for a duration (e.g., after death, during transitions).
        /// </summary>
        public void DisableInputForDuration(float seconds) {
            StartCoroutine(DisableInputCoroutine(seconds));
        }

        private System.Collections.IEnumerator DisableInputCoroutine(float seconds) {
            DisableAllInput();
            yield return new WaitForSecondsRealtime(seconds);
            EnableAllInput();
        }
    }
}

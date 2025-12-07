using System;
using UnityEngine;

namespace KadaXuanwu.Utils.Runtime.FPController.Core {
    /// <summary>
    /// Core character events that apply regardless of which modifiers are active.
    /// </summary>
    public class CharacterEvents {
        /// <summary>
        /// Fired when grounded state changes.
        /// </summary>
        public event Action<bool> OnGroundedChanged;

        /// <summary>
        /// Fired when velocity changes significantly due to collision.
        /// </summary>
        public event Action<Vector3, Vector3> OnVelocityImpact;

        /// <summary>
        /// Fired when the player is teleported.
        /// </summary>
        public event Action<Vector3> OnTeleported;

        public void InvokeGroundedChanged(bool isGrounded) => OnGroundedChanged?.Invoke(isGrounded);
        public void InvokeVelocityImpact(Vector3 previous, Vector3 current) => OnVelocityImpact?.Invoke(previous, current);
        public void InvokeTeleported(Vector3 position) => OnTeleported?.Invoke(position);

        public void ClearAllSubscribers() {
            OnGroundedChanged = null;
            OnVelocityImpact = null;
            OnTeleported = null;
        }
    }
}

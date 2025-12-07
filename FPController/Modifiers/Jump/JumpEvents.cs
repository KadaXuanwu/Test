using System;
using KadaXuanwu.Utils.Runtime.FPController.Core.Interfaces;
using UnityEngine;

namespace KadaXuanwu.Utils.Runtime.FPController.Modifiers.Jump {
    public readonly struct JumpEventData {
        public readonly Vector3 Position;
        public readonly Vector3 Velocity;
        public readonly bool WasGrounded;

        public JumpEventData(Vector3 position, Vector3 velocity, bool wasGrounded) {
            Position = position;
            Velocity = velocity;
            WasGrounded = wasGrounded;
        }
    }

    public class JumpEvents : IModifierEvents {
        public event Action<JumpEventData> OnJumped;

        public void InvokeJumped(JumpEventData data) => OnJumped?.Invoke(data);

        public void ClearSubscribers() {
            OnJumped = null;
        }
    }
}

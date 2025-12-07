using System;
using KadaXuanwu.Utils.Runtime.FPController.Core.Interfaces;
using UnityEngine;

namespace KadaXuanwu.Utils.Runtime.FPController.Modifiers.Landing {
    public readonly struct LandingEventData {
        public readonly float FallVelocity;
        public readonly float AirTime;
        public readonly Vector3 Position;
        public readonly float DampeningApplied;

        public LandingEventData(float fallVelocity, float airTime, Vector3 position, float dampeningApplied) {
            FallVelocity = fallVelocity;
            AirTime = airTime;
            Position = position;
            DampeningApplied = dampeningApplied;
        }
    }

    public class LandingEvents : IModifierEvents {
        /// <summary>
        /// Fired when the player lands.
        /// </summary>
        public event Action<LandingEventData> OnLanded;

        /// <summary>
        /// Fired when a hard landing occurs (high fall velocity).
        /// </summary>
        public event Action<LandingEventData> OnHardLanding;

        public void InvokeLanded(LandingEventData data) => OnLanded?.Invoke(data);
        public void InvokeHardLanding(LandingEventData data) => OnHardLanding?.Invoke(data);

        public void ClearSubscribers() {
            OnLanded = null;
            OnHardLanding = null;
        }
    }
}

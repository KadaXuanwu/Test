using System;
using KadaXuanwu.Utils.Runtime.FPController.Core.Interfaces;
using UnityEngine;

namespace KadaXuanwu.Utils.Runtime.FPController.Modifiers.Sliding {
    public readonly struct SlidingEventData {
        public readonly float SlopeAngle;
        public readonly Vector3 SlopeNormal;
        public readonly Vector3 SlideDirection;
        public readonly float CurrentSpeed;

        public SlidingEventData(float slopeAngle, Vector3 slopeNormal, Vector3 slideDirection, float currentSpeed) {
            SlopeAngle = slopeAngle;
            SlopeNormal = slopeNormal;
            SlideDirection = slideDirection;
            CurrentSpeed = currentSpeed;
        }
    }

    public class SlidingEvents : IModifierEvents {
        /// <summary>
        /// Fired when sliding begins.
        /// </summary>
        public event Action<SlidingEventData> OnSlidingStarted;

        /// <summary>
        /// Fired when sliding ends.
        /// </summary>
        public event Action OnSlidingEnded;

        /// <summary>
        /// Fired every frame while sliding.
        /// </summary>
        public event Action<SlidingEventData> OnSlidingUpdate;

        /// <summary>
        /// Fired when sliding stops due to getting stuck.
        /// </summary>
        public event Action OnSlidingStuck;

        public void InvokeSlidingStarted(SlidingEventData data) => OnSlidingStarted?.Invoke(data);
        public void InvokeSlidingEnded() => OnSlidingEnded?.Invoke();
        public void InvokeSlidingUpdate(SlidingEventData data) => OnSlidingUpdate?.Invoke(data);
        public void InvokeSlidingStuck() => OnSlidingStuck?.Invoke();

        public void ClearSubscribers() {
            OnSlidingStarted = null;
            OnSlidingEnded = null;
            OnSlidingUpdate = null;
            OnSlidingStuck = null;
        }
    }
}

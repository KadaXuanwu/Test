using KadaXuanwu.Utils.Runtime.FPController.Core.Interfaces;
using UnityEngine;

namespace KadaXuanwu.Utils.Runtime.FPController.Modifiers.Sprint {
    [CreateAssetMenu(fileName = "SprintConfig", menuName = "Character/Modifiers/Sprint")]
    public class SprintConfig : ScriptableObject, IModifierConfig {
        [Tooltip("Speed multiplier while running.")]
        [Min(1f)] public float SpeedMultiplier = 1.85f;

        [Tooltip("If true, player must be moving forward to run.")]
        public bool RequireForwardMovement = true;

        public IMovementModifier CreateModifier() => new SprintModifier(this);
    }
}

using KadaXuanwu.Utils.Runtime.FPController.Core.Interfaces;
using UnityEngine;

namespace KadaXuanwu.Utils.Runtime.FPController.Modifiers.Jump {
    [CreateAssetMenu(fileName = "JumpConfig", menuName = "Character/Modifiers/Jump")]
    public class JumpConfig : ScriptableObject, IModifierConfig {
        [Header("Jump")]
        [Tooltip("Height of the jump in units.")]
        [Min(0f)] public float JumpHeight = 3f;

        [Tooltip("Horizontal speed multiplier when jumping.")]
        [Min(1f)] public float JumpSpeedBoostFactor = 1.05f;

        [Tooltip("Minimum time between jumps.")]
        [Min(0f)] public float JumpCooldown = 0.4f;

        [Header("Coyote Time")]
        [Tooltip("Time after leaving ground where jump is still allowed.")]
        [Min(0f)] public float CoyoteTime = 0.1f;

        public IMovementModifier CreateModifier() => new JumpModifier(this);
    }
}

using UnityEngine;

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

    [Header("Air Jump")]
    [Tooltip("Number of additional jumps allowed while airborne.")]
    [Min(0)] public int MaxAirJumps = 0;

    [Tooltip("Damping applied to downward velocity when jumping mid-air.")]
    [Min(0f)] public float MidAirVelocityDamping = 5f;
}

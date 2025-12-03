using UnityEngine;

[CreateAssetMenu(fileName = "SlidingConfig", menuName = "Character/Modifiers/Sliding")]
public class SlidingConfig : ScriptableObject, IModifierConfig {
    [Header("Speed")]
    [Tooltip("Base sliding speed on slopes.")]
    [Min(0f)] public float SlidingSpeed = 50f;

    [Tooltip("Magnitude of downward force projection for slide direction.")]
    [Min(0f)] public float SlideProjectionMagnitude = 5f;

    [Header("Control")]
    [Tooltip("How much horizontal input affects sliding (0 = none, 1 = full).")]
    [Range(0f, 1f)] public float MovementControlXZ = 0.6f;

    [Tooltip("How much vertical velocity is preserved while sliding.")]
    [Range(0f, 1f)] public float MovementControlY = 0.5f;

    [Tooltip("Constant downward velocity added while sliding.")]
    [Min(0f)] public float YVelocityNudge = 0.1f;

    [Header("Timing")]
    [Tooltip("Delay before sliding begins on a slope.")]
    [Min(0f)] public float TimeToStart = 0.3f;

    [Tooltip("Time to reach full sliding speed after start delay.")]
    [Min(0f)] public float TimeToFullSpeed = 0.6f;

    [Header("Stuck Detection")]
    [Tooltip("Movement threshold to detect if stuck.")]
    [Min(0f)] public float StuckThreshold = 0.04f;

    [Tooltip("Frames below threshold before stopping slide.")]
    [Min(1)] public int StoppedFrameThreshold = 3;

    public IMovementModifier CreateModifier() => new SlidingModifier(this);
}

using UnityEngine;

[CreateAssetMenu(fileName = "CrouchConfig", menuName = "Character/Modifiers/Crouch")]
public class CrouchConfig : ScriptableObject, IModifierConfig {
    [Header("Speed")]
    [Tooltip("Speed multiplier while crouching.")]
    [Range(0f, 1f)] public float SpeedMultiplier = 0.65f;

    [Header("Controller")]
    [Tooltip("Character controller height when standing.")]
    [Min(0f)] public float StandingHeight = 1.75f;

    [Tooltip("Character controller height when crouching.")]
    [Min(0f)] public float CrouchingHeight = 1.4f;

    [Tooltip("Controller center Y position when standing.")]
    public float StandingCenterY = -0.05f;

    [Tooltip("Controller center Y position when crouching.")]
    public float CrouchingCenterY = -0.25f;

    [Header("Camera")]
    [Tooltip("Camera Y position when standing.")]
    public float CameraStandingY = 0.7f;

    [Tooltip("Camera Y position when crouching.")]
    public float CameraCrouchingY = 0.35f;

    [Tooltip("Speed of camera transition when crouching/standing.")]
    [Min(0f)] public float TransitionSpeed = 3f;
}

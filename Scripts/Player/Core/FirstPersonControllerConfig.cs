using UnityEngine;

[CreateAssetMenu(fileName = "FPControllerConfig", menuName = "Character/Controller Config")]
public class FirstPersonControllerConfig : ScriptableObject {
    [Header("Physics")]
    [Tooltip("Gravity multiplier (1 = realistic, 2 = faster fall).")]
    [Min(0f)] public float GravityMultiplier = 2f;

    [Tooltip("Downward velocity applied when grounded to maintain ground contact.")]
    [Min(0f)] public float GroundedSnapVelocity = 0.01f;

    [Header("Look")]
    [Tooltip("Mouse sensitivity multiplier.")]
    [Min(0f)] public float MouseSensitivity = 2f;

    [Tooltip("Maximum vertical look angle.")]
    [Range(0f, 90f)] public float ClampAngle = 89.999f;

    [Header("Ground Check")]
    public LayerMask GroundLayers;

    [Tooltip("Number of raycasts around the player for ground detection.")]
    [Range(1, 12)] public int GroundCheckCount = 6;

    [Tooltip("Radius of the ground check circle.")]
    [Min(0f)] public float GroundCheckRadius = 0.22f;

    [Tooltip("Distance of ground check raycasts.")]
    [Min(0f)] public float GroundRaycastDistance = 1.1f;

    [Tooltip("Magnitude of downward force for slope slide direction calculation.")]
    [Min(0f)] public float SlopeSlideProjectionMagnitude = 5f;

    [Header("Moving Platforms")]
    public string MovingPlatformTag = "MovingPlatform";

    [Tooltip("How much platform velocity is inherited (0 = none, 1 = full).")]
    [Range(0f, 1f)] public float PlatformVelocityInheritance = 1f;

    [Tooltip("How much platform rotation is inherited (0 = none, 1 = full).")]
    [Range(0f, 1f)] public float PlatformRotationInheritance = 1f;

    [Tooltip("Maximum speed inherited from platforms.")]
    [Min(0f)] public float MaxInheritedPlatformSpeed = 50f;
}

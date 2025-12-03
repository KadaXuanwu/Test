using UnityEngine;

[CreateAssetMenu(fileName = "FPControllerConfig", menuName = "Character/Controller Config")]
public class FirstPersonControllerConfig : ScriptableObject {
    [Header("Physics")]
    [Tooltip("Gravity multiplier.")]
    [Min(0f)] public float GravityMultiplier = 2f;

    [Tooltip("Downward velocity applied when grounded to maintain ground contact.")]
    [Min(0f)] public float GroundedSnapVelocity = 0.01f;

    [Header("Look")]
    [Tooltip("Mouse sensitivity multiplier.")]
    [Min(0f)] public float MouseSensitivity = 5f;

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
}

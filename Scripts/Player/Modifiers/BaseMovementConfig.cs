using UnityEngine;

[CreateAssetMenu(fileName = "BaseMovementConfig", menuName = "Character/Modifiers/Base Movement")]
public class BaseMovementConfig : ScriptableObject, IModifierConfig {
    [Header("Speed")]
    [Tooltip("Maximum horizontal movement speed.")]
    [Min(0f)] public float MaxSpeed = 5.5f;

    [Tooltip("Speed below which the character stops completely.")]
    [Min(0f)] public float StoppingSpeed = 0.5f;

    [Header("Acceleration")]
    [Tooltip("How quickly the character reaches max speed.")]
    [Min(0f)] public float Acceleration = 4f;

    [Tooltip("How quickly the character slows down when changing direction.")]
    [Min(0f)] public float DecelerationForce = 9.6f;

    [Header("Friction")]
    [Tooltip("Friction applied while grounded and moving.")]
    [Min(0f)] public float GroundFriction = 5f;

    [Tooltip("Additional friction when stopping or over max speed.")]
    [Min(0f)] public float GroundStoppingFriction = 2.5f;

    [Tooltip("Friction applied while airborne.")]
    [Min(0f)] public float AirFriction = 0.6f;

    [Header("Air Control")]
    [Tooltip("Movement effectiveness while airborne (0 = none, 1 = full).")]
    [Range(0f, 1f)] public float AirControlFactor = 0.2f;

    [Tooltip("Time to reach full air control after leaving ground.")]
    [Min(0f)] public float TimeToFullAirControl = 0.1f;

    [Header("Sprint Bonus")]
    [Tooltip("Minimum speed to maintain sprint bonus.")]
    [Min(0f)] public float MinRunVelocityForSprint = 6f;

    [Tooltip("Time at high speed before sprint bonus activates.")]
    [Min(0f)] public float MinRunDurationForSprint = 0.5f;

    [Tooltip("Movement effectiveness multiplier during sprint bonus.")]
    [Min(1f)] public float SprintBonusVelocityFactor = 1.2f;

    public IMovementModifier CreateModifier() => new BaseMovementModifier(this);
}
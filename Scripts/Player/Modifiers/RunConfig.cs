using UnityEngine;

[CreateAssetMenu(fileName = "RunConfig", menuName = "Character/Modifiers/Run")]
public class RunConfig : ScriptableObject, IModifierConfig {
    [Tooltip("Speed multiplier while running.")]
    [Min(1f)] public float SpeedMultiplier = 1.85f;

    [Tooltip("If true, player must be moving forward to run.")]
    public bool RequireForwardMovement = true;
}

/// <summary>
/// Interface for pluggable movement mechanics.
/// Implement this to add new behaviors like double jump, dash, wall run, etc.
/// </summary>
public interface IMovementModifier {
    /// <summary>
    /// Execution order. Lower values run first.
    /// Recommended ranges:
    /// -100 to -1: Pre-processing (input modification, platform inheritance)
    /// 0 to 99: Core movement (base movement, jumping, crouching)
    /// 100 to 199: Movement modifiers (sliding, dashing)
    /// 200+: Post-processing (clamping, validation)
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Whether this modifier is currently active.
    /// Inactive modifiers are skipped during processing.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Called each frame to process movement.
    /// Modify the context to affect the final velocity.
    /// </summary>
    void ProcessMovement(ref MovementContext context);

    /// <summary>
    /// Called when the modifier is added to the controller.
    /// </summary>
    void OnInitialize(FirstPersonController controller);

    /// <summary>
    /// Called when the modifier is removed from the controller.
    /// </summary>
    void OnRemove();
}

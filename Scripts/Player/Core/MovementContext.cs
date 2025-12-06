using UnityEngine;

/// <summary>
/// Context passed to movement modifiers each frame.
/// Modifiers read and write to this to affect movement.
/// </summary>
public struct MovementContext {
    // === Input (set by controller, read by modifiers) ===
    public Vector3 MoveInput;
    public Vector3 WorldMoveDirection;
    public Vector3 Position;
    public bool IsGrounded;
    public bool WasGroundedLastFrame;
    public GroundInfo GroundInfo;
    public float DeltaTime;
    public float PreviousYVelocity;

    // === Output (modified by modifiers) ===
    public Vector3 Velocity;
    public float SpeedMultiplier;
    public bool PreventMovement;
    public bool PreventGravity;

    // === Extensible modifier state ===
    /// <summary>
    /// Container for modifier-specific state. Use State.GetOrCreate&lt;T&gt;() to access.
    /// </summary>
    public ModifierStateContainer State;
}

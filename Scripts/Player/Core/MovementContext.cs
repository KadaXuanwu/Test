using UnityEngine;

/// <summary>
/// Context passed to movement modifiers each frame.
/// Modifiers read and write to this to affect movement.
/// </summary>
public struct MovementContext {
    // Input state
    public Vector3 MoveInput;
    public Vector3 WorldMoveDirection;
    public float MaxSpeed;

    // Current state
    public Vector3 Velocity;
    public Vector3 Position;
    public bool IsGrounded;
    public bool WasGroundedLastFrame;
    public GroundInfo GroundInfo;
    public float DeltaTime;

    // State flags (modifiers can read/write)
    public bool IsCrouching;
    public bool IsRunning;

    // Control flags
    public bool PreventMovement;
    public bool PreventGravity;
    public bool ConsumedJump;

    // Velocity from previous frame (for landing calculations, etc.)
    public float PreviousYVelocity;
}

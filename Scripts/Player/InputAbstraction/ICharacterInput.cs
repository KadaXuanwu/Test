using UnityEngine;

/// <summary>
/// Abstraction for character input.
/// Implement this interface to support different input systems.
/// </summary>
public interface ICharacterInput {
    Vector2 MoveInput { get; }
    Vector2 LookInput { get; }
    bool JumpPressed { get; }
    bool JumpHeld { get; }
    bool RunPressed { get; }
    bool CrouchPressed { get; }
}
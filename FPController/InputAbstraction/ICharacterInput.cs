using UnityEngine;

namespace KadaXuanwu.Utils.Runtime.FPController.InputAbstraction {
    /// <summary>
    /// Abstraction for character input.
    /// Implement this interface to support different input systems.
    /// </summary>
    public interface ICharacterInput {
        Vector2 MoveInput { get; }
        Vector2 LookInput { get; }
        bool JumpPressed { get; }
        bool JumpHeld { get; }
        bool SprintPressed { get; }
        bool CrouchPressed { get; }
    }
}

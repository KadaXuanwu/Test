using System;
using UnityEngine;

public readonly struct JumpEventData {
    public readonly Vector3 Position;
    public readonly Vector3 Velocity;
    public readonly bool WasGrounded;
    public readonly bool WasAirJump;
    public readonly int AirJumpsRemaining;

    public JumpEventData(Vector3 position, Vector3 velocity, bool wasGrounded, bool wasAirJump, int airJumpsRemaining) {
        Position = position;
        Velocity = velocity;
        WasGrounded = wasGrounded;
        WasAirJump = wasAirJump;
        AirJumpsRemaining = airJumpsRemaining;
    }
}

public class JumpEvents : IModifierEvents {
    /// <summary>
    /// Fired when a jump is executed.
    /// </summary>
    public event Action<JumpEventData> OnJumped;

    /// <summary>
    /// Fired when air jump count is reset (on landing).
    /// </summary>
    public event Action<int> OnAirJumpsReset;

    /// <summary>
    /// Fired when an air jump is consumed.
    /// </summary>
    public event Action<int> OnAirJumpUsed;

    public void InvokeJumped(JumpEventData data) => OnJumped?.Invoke(data);
    public void InvokeAirJumpsReset(int count) => OnAirJumpsReset?.Invoke(count);
    public void InvokeAirJumpUsed(int remaining) => OnAirJumpUsed?.Invoke(remaining);

    public void ClearSubscribers() {
        OnJumped = null;
        OnAirJumpsReset = null;
        OnAirJumpUsed = null;
    }
}

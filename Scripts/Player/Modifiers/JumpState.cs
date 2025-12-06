/// <summary>
/// State for JumpModifier. Implements IResettableState because ConsumedJump resets each frame.
/// </summary>
public class JumpState : IResettableState {
    /// <summary>
    /// Set to true when a modifier consumes the jump input this frame.
    /// Prevents multiple modifiers from responding to the same jump press.
    /// </summary>
    public bool ConsumedJump;

    /// <summary>
    /// Remaining air jumps available.
    /// </summary>
    public int AirJumpsRemaining;

    public void Reset() {
        ConsumedJump = false;
    }
}

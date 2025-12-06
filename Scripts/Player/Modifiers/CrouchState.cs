/// <summary>
/// State for CrouchModifier. Other modifiers can read this to check crouch status.
/// </summary>
public class CrouchState {
    public bool IsCrouching;
    public bool WasCrouchingLastFrame;
}

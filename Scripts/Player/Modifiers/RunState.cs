/// <summary>
/// State for RunModifier. Other modifiers can read this to check run status.
/// </summary>
public class RunState {
    public bool IsRunning;
    public bool WasRunningLastFrame;
}

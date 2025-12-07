namespace KadaXuanwu.Utils.Runtime.FPController.Modifiers.Sprint {
    /// <summary>
    /// State for RunModifier. Other modifiers can read this to check run status.
    /// </summary>
    public class SprintState {
        public bool IsRunning;
        public bool WasRunningLastFrame;
    }
}

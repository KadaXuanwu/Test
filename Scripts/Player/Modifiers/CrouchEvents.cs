using System;

public class CrouchEvents : IModifierEvents {
    /// <summary>
    /// Fired when crouch state changes.
    /// </summary>
    public event Action<bool> OnCrouchChanged;

    /// <summary>
    /// Fired when crouch is blocked (e.g., obstacle above).
    /// </summary>
    public event Action OnCrouchBlocked;

    public void InvokeCrouchChanged(bool isCrouching) => OnCrouchChanged?.Invoke(isCrouching);
    public void InvokeCrouchBlocked() => OnCrouchBlocked?.Invoke();

    public void ClearSubscribers() {
        OnCrouchChanged = null;
        OnCrouchBlocked = null;
    }
}

using UnityEngine;

public static class PhysicsHelpers {
    /// <summary>
    /// Returns a multiplier for frame-rate independent damping.
    /// damping: decay rate (higher = faster decay)
    /// </summary>
    public static float GetDampingMultiplier(float damping, float deltaTime) {
        return Mathf.Exp(-damping * deltaTime);
    }
}
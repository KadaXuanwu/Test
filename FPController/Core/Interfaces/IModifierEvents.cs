namespace KadaXuanwu.Utils.Runtime.FPController.Core.Interfaces {
    /// <summary>
    /// Interface for modifiers that expose events.
    /// </summary>
    public interface IModifierEvents {
        /// <summary>
        /// Clears all event subscribers.
        /// Called automatically when modifier is removed.
        /// </summary>
        void ClearSubscribers();
    }
}

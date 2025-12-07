using System;
using KadaXuanwu.Utils.Runtime.FPController.Core.Interfaces;

namespace KadaXuanwu.Utils.Runtime.FPController.Modifiers.Sprint {
    public class SprintEvents : IModifierEvents {
        /// <summary>
        /// Fired when running state changes.
        /// </summary>
        public event Action<bool> OnRunningChanged;

        public void InvokeRunningChanged(bool isRunning) => OnRunningChanged?.Invoke(isRunning);

        public void ClearSubscribers() {
            OnRunningChanged = null;
        }
    }
}

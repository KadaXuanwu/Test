using KadaXuanwu.Utils.Runtime.FPController.Core;

namespace KadaXuanwu.Utils.Runtime.FPController.Modifiers.Jump {
    public class JumpState : IResettableState {
        public bool ConsumedJump;

        public void Reset() {
            ConsumedJump = false;
        }
    }
}

using KadaXuanwu.Utils.Runtime.FPController.Core;
using KadaXuanwu.Utils.Runtime.FPController.Modifiers.Base;
using KadaXuanwu.Utils.Runtime.FPController.Modifiers.Crouch;

namespace KadaXuanwu.Utils.Runtime.FPController.Modifiers.Sprint {
    public class SprintModifier : MovementModifierBase<SprintConfig, SprintEvents> {
        public SprintModifier(SprintConfig config) : base(config) { }

        public override void ProcessMovement(ref MovementContext context) {
            SprintState state = context.State.GetOrCreate<SprintState>();

            bool wantsToRun = Input.SprintPressed;
            bool movingForward = !Config.RequireForwardMovement || context.MoveInput.z > 0f;

            // Check if crouching (optional dependency)
            bool isCrouching = false;
            if (context.State.TryGet<CrouchState>(out CrouchState crouchState)) {
                isCrouching = crouchState.IsCrouching;
            }

            bool canRun = !isCrouching && movingForward;
            state.IsRunning = wantsToRun && canRun;

            if (state.IsRunning) {
                context.SpeedMultiplier *= Config.SpeedMultiplier;
            }

            if (state.IsRunning != state.WasRunningLastFrame) {
                Events.InvokeRunningChanged(state.IsRunning);
            }

            state.WasRunningLastFrame = state.IsRunning;
        }
    }
}

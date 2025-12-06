public class RunModifier : MovementModifierBase<RunConfig, RunEvents> {
    public RunModifier(RunConfig config) : base(config) { }

    public override void ProcessMovement(ref MovementContext context) {
        RunState state = context.State.GetOrCreate<RunState>();

        bool wantsToRun = Input.RunPressed;
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

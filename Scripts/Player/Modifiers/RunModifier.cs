using UnityEngine;

public class RunModifier : MovementModifierBase<RunConfig, RunEvents> {
    private bool _wasRunningLastFrame;

    public RunModifier(RunConfig config) : base(config) { }

    public override void ProcessMovement(ref MovementContext context) {
        bool wantsToRun = Input.RunPressed;
        bool movingForward = !Config.RequireForwardMovement || context.MoveInput.z > 0f;
        bool canRun = !context.IsCrouching && movingForward;

        context.IsRunning = wantsToRun && canRun;

        if (context.IsRunning) {
            // TODO move faster
        }

        if (context.IsRunning != _wasRunningLastFrame) {
            Events.InvokeRunningChanged(context.IsRunning);
        }

        _wasRunningLastFrame = context.IsRunning;
    }
}

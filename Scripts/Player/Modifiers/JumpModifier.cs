using UnityEngine;

public class JumpModifier : MovementModifierBase<JumpConfig, JumpEvents> {
    private float _lastJumpTimestamp;

    public JumpModifier(JumpConfig config) : base(config) { }

    public override void ProcessMovement(ref MovementContext context) {
        JumpState state = context.State.GetOrCreate<JumpState>();

        // Reset air jumps on landing
        if (context.IsGrounded && !context.WasGroundedLastFrame) {
            state.AirJumpsRemaining = Config.MaxAirJumps;
            Events.InvokeAirJumpsReset(Config.MaxAirJumps);
        }

        if (!Input.JumpPressed || state.ConsumedJump) {
            return;
        }

        bool onCooldown = _lastJumpTimestamp + Config.JumpCooldown > Time.time;
        if (onCooldown) {
            return;
        }

        float lastGrounded = Controller.GetLastGroundedTimestamp();
        bool inCoyoteTime = lastGrounded + Config.CoyoteTime > Time.time;
        bool canAirJump = !context.IsGrounded && !inCoyoteTime && state.AirJumpsRemaining > 0;

        if (!context.IsGrounded && !inCoyoteTime && !canAirJump) {
            return;
        }

        bool isAirJump = !context.IsGrounded && !inCoyoteTime && canAirJump;

        if (isAirJump) {
            state.AirJumpsRemaining--;
            Events.InvokeAirJumpUsed(state.AirJumpsRemaining);
        }

        ExecuteJump(ref context, state, isAirJump);
    }

    private void ExecuteJump(ref MovementContext context, JumpState state, bool isAirJump) {
        _lastJumpTimestamp = Time.time;
        state.ConsumedJump = true;

        float yVelocity = context.Velocity.y;

        if (yVelocity < 0f) {
            yVelocity *= PhysicsHelpers.GetDampingMultiplier(Config.MidAirVelocityDamping, context.DeltaTime);
        }

        float jumpVelocity = Mathf.Sqrt(-Config.JumpHeight * -9.81f * BaseConfig.GravityMultiplier);
        yVelocity += jumpVelocity;

        Vector3 horizontalVelocity = new Vector3(context.Velocity.x, 0f, context.Velocity.z);
        horizontalVelocity *= Config.JumpSpeedBoostFactor;

        context.Velocity = new Vector3(horizontalVelocity.x, yVelocity, horizontalVelocity.z);

        JumpEventData eventData = new JumpEventData(
            context.Position,
            context.Velocity,
            context.WasGroundedLastFrame,
            isAirJump,
            state.AirJumpsRemaining
        );
        Events.InvokeJumped(eventData);
    }
}

using UnityEngine;

public class JumpModifier : MovementModifierBase<JumpConfig, JumpEvents> {
    public int AirJumpsRemaining => _airJumpsRemaining;

    private float _lastJumpTimestamp;
    private int _airJumpsRemaining;

    public JumpModifier(JumpConfig config) : base(config) { }

    public override void ProcessMovement(ref MovementContext context) {
        if (context.IsGrounded && !context.WasGroundedLastFrame) {
            _airJumpsRemaining = Config.MaxAirJumps;
            Events.InvokeAirJumpsReset(Config.MaxAirJumps);
        }

        if (!Input.JumpPressed || context.ConsumedJump) {
            return;
        }

        bool onCooldown = _lastJumpTimestamp + Config.JumpCooldown > Time.time;
        if (onCooldown) {
            return;
        }

        float lastGrounded = Controller.GetLastGroundedTimestamp();
        bool inCoyoteTime = lastGrounded + Config.CoyoteTime > Time.time;
        bool canAirJump = !context.IsGrounded && !inCoyoteTime && _airJumpsRemaining > 0;

        if (!context.IsGrounded && !inCoyoteTime && !canAirJump) {
            return;
        }

        bool isAirJump = !context.IsGrounded && !inCoyoteTime && canAirJump;

        if (isAirJump) {
            _airJumpsRemaining--;
            Events.InvokeAirJumpUsed(_airJumpsRemaining);
        }

        ExecuteJump(ref context, isAirJump);
    }

    private void ExecuteJump(ref MovementContext context, bool isAirJump) {
        _lastJumpTimestamp = Time.time;
        context.ConsumedJump = true;

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
            _airJumpsRemaining
        );
        Events.InvokeJumped(eventData);
    }
}

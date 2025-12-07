using KadaXuanwu.Utils.Runtime.FPController.Core;
using KadaXuanwu.Utils.Runtime.FPController.Modifiers.Base;
using UnityEngine;

namespace KadaXuanwu.Utils.Runtime.FPController.Modifiers.Jump {
    public class JumpModifier : MovementModifierBase<JumpConfig, JumpEvents> {
        private float _lastJumpTimestamp;
        private bool _jumpHeldConsumed;

        public JumpModifier(JumpConfig config) : base(config) { }

        public override void ProcessMovement(ref MovementContext context) {
            JumpState state = context.State.GetOrCreate<JumpState>();

            if (!Input.JumpHeld)
                _jumpHeldConsumed = false;
            else if (context.IsGrounded && !context.WasGroundedLastFrame)
                _jumpHeldConsumed = false;

            if (!Input.JumpHeld || _jumpHeldConsumed || state.ConsumedJump)
                return;

            if (_lastJumpTimestamp + Config.JumpCooldown > Time.time)
                return;

            float lastGrounded = Controller.GetLastGroundedTimestamp();
            bool inCoyoteTime = lastGrounded + Config.CoyoteTime > Time.time
                               && _lastJumpTimestamp < lastGrounded;

            bool canJump = context.IsGrounded || inCoyoteTime;

            if (!canJump)
                return;

            ExecuteJump(ref context, state);
        }

        private void ExecuteJump(ref MovementContext context, JumpState state) {
            _lastJumpTimestamp = Time.time;
            _jumpHeldConsumed = true;
            state.ConsumedJump = true;

            float jumpVelocity = Mathf.Sqrt(-Config.JumpHeight * -9.81f * BaseConfig.GravityMultiplier);
            Vector3 horizontalVelocity = new Vector3(context.Velocity.x, 0f, context.Velocity.z) * Config.JumpSpeedBoostFactor;

            context.Velocity = new Vector3(horizontalVelocity.x, jumpVelocity, horizontalVelocity.z);

            JumpEventData eventData = new JumpEventData(context.Position, context.Velocity, context.WasGroundedLastFrame);
            Events.InvokeJumped(eventData);
        }
    }
}

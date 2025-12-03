using UnityEngine;

public class LandingModifier : MovementModifierBase<LandingConfig, LandingEvents> {
    private float _timestampAirborneStarted;

    public LandingModifier(LandingConfig config) : base(config) { }

    public override void ProcessMovement(ref MovementContext context) {
        if (!context.IsGrounded) {
            if (context.WasGroundedLastFrame) {
                _timestampAirborneStarted = Time.time;
            }
            return;
        }

        if (!context.WasGroundedLastFrame) {
            HandleLanding(ref context);
        }
    }

    private void HandleLanding(ref MovementContext context) {
        float airTime = Time.time - _timestampAirborneStarted;
        float fallVelocity = context.PreviousYVelocity;

        float dampeningApplied = 0f;

        if (Mathf.Abs(fallVelocity) >= Config.MinImpactVelocity) {
            dampeningApplied = Mathf.Lerp(1f, 0f, Mathf.Abs(fallVelocity / Config.VelocityDivisor));
            Vector3 horizontalVelocity = new Vector3(context.Velocity.x, 0f, context.Velocity.z);
            horizontalVelocity *= dampeningApplied;
            context.Velocity = new Vector3(horizontalVelocity.x, context.Velocity.y, horizontalVelocity.z);
        }

        LandingEventData eventData = new LandingEventData(fallVelocity, airTime, context.Position, dampeningApplied);
        Events.InvokeLanded(eventData);

        if (Mathf.Abs(fallVelocity) > Config.VelocityDivisor * 0.5f) {
            Events.InvokeHardLanding(eventData);
        }
    }
}

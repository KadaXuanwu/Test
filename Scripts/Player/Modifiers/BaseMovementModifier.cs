using UnityEngine;

public class BaseMovementModifier : MovementModifierBase<BaseMovementConfig> {
    public override int Priority => 0;

    private float _lastHighVelocityLostTimestamp;
    private float _timestampAirborneStarted;

    public BaseMovementModifier(BaseMovementConfig config) : base(config) { }

    public override void ProcessMovement(ref MovementContext context) {
        Vector3 moveVelocity = new Vector3(context.Velocity.x, 0f, context.Velocity.z);

        UpdateAirborneTracking(context.IsGrounded);
        UpdateSprintBonusTracking(moveVelocity.magnitude);

        Vector3 moveInput = context.WorldMoveDirection * context.MaxSpeed;
        float effectiveness = CalculateEffectiveness(context.IsGrounded);

        moveVelocity = ApplyAcceleration(moveVelocity, moveInput, effectiveness, context.DeltaTime);
        moveVelocity = ClampSpeed(moveVelocity, context.MaxSpeed);
        moveVelocity = ApplyFriction(moveVelocity, moveInput, context.MaxSpeed, context.IsGrounded, context.DeltaTime);

        if (moveVelocity.magnitude < Config.StoppingSpeed) {
            moveVelocity = Vector3.zero;
        }

        context.Velocity = new Vector3(moveVelocity.x, context.Velocity.y, moveVelocity.z);
    }

    private void UpdateAirborneTracking(bool isGrounded) {
        if (isGrounded) {
            _timestampAirborneStarted = Time.time;
        }
    }

    private void UpdateSprintBonusTracking(float currentSpeed) {
        if (currentSpeed < Config.MinRunVelocityForSprint) {
            _lastHighVelocityLostTimestamp = Time.time;
        }
    }

    private float CalculateEffectiveness(bool isGrounded) {
        float effectiveness = 1f;

        if (!isGrounded) {
            float airProgress = Mathf.Clamp01((Time.time - _timestampAirborneStarted) / Config.TimeToFullAirControl);
            effectiveness = Mathf.Lerp(1f, Config.AirControlFactor, airProgress);
        }

        bool hasSprintBonus = _lastHighVelocityLostTimestamp + Config.MinRunDurationForSprint < Time.time;
        if (hasSprintBonus) {
            effectiveness *= Config.SprintBonusVelocityFactor;
        }

        return effectiveness;
    }

    private Vector3 ApplyAcceleration(Vector3 velocity, Vector3 input, float effectiveness, float deltaTime) {
        Vector3 accelerated = velocity + input * (effectiveness * Config.Acceleration * deltaTime);

        if (accelerated.magnitude < velocity.magnitude) {
            return velocity + input * (effectiveness * Config.DecelerationForce * deltaTime);
        }

        return accelerated;
    }

    private Vector3 ClampSpeed(Vector3 velocity, float maxSpeed) {
        return velocity.magnitude > maxSpeed
            ? Vector3.ClampMagnitude(velocity, maxSpeed)
            : velocity;
    }

    private Vector3 ApplyFriction(Vector3 velocity, Vector3 input, float maxSpeed, bool grounded, float deltaTime) {
        if (grounded) {
            velocity *= PhysicsHelpers.GetDampingMultiplier(Config.GroundFriction, deltaTime);

            if (input == Vector3.zero || velocity.magnitude > maxSpeed) {
                velocity *= PhysicsHelpers.GetDampingMultiplier(Config.GroundStoppingFriction, deltaTime);
            }
        }
        else {
            velocity *= PhysicsHelpers.GetDampingMultiplier(Config.AirFriction, deltaTime);
        }

        return velocity;
    }
}

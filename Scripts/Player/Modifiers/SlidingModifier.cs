using UnityEngine;

public class SlidingModifier : MovementModifierBase<SlidingConfig, SlidingEvents> {
    public bool IsSliding => _phase != SlidingPhase.None;

    private enum SlidingPhase {
        None,
        WarmingUp,
        Ramping,
        Full
    }

    private SlidingPhase _phase;
    private float _startTimestamp;
    private int _stuckFrameCount;
    private Vector3 _lastPosition;
    private bool _wasSlidingLastFrame;

    public SlidingModifier(SlidingConfig config) : base(config) { }

    public override void ProcessMovement(ref MovementContext context) {
        _lastPosition = context.Position;

        bool shouldSlide = context.GroundInfo.OnGround &&
                           context.GroundInfo.OnSlope &&
                           context.Velocity.y <= 0f;

        if (!shouldSlide) {
            if (_phase != SlidingPhase.None) {
                EndSliding(false);
            }
            UpdateEvents(context);
            return;
        }

        UpdatePhase();
        ApplySliding(ref context);
        UpdateEvents(context);
    }

    private void UpdatePhase() {
        if (_phase == SlidingPhase.None) {
            _phase = SlidingPhase.WarmingUp;
            _startTimestamp = Time.time;
            _stuckFrameCount = 0;
            return;
        }

        float elapsed = Time.time - _startTimestamp;

        if (elapsed <= Config.TimeToStart) {
            _phase = SlidingPhase.WarmingUp;
        }
        else if (elapsed <= Config.TimeToStart + Config.TimeToFullSpeed) {
            _phase = SlidingPhase.Ramping;
        }
        else {
            _phase = SlidingPhase.Full;
        }
    }

    private void ApplySliding(ref MovementContext context) {
        if (_phase == SlidingPhase.None || _phase == SlidingPhase.WarmingUp) {
            return;
        }

        Vector3 slideForce = context.GroundInfo.SlideDirection *
                             (context.DeltaTime * Config.SlidingSpeed * Config.SlideProjectionMagnitude);

        if (_phase == SlidingPhase.Ramping) {
            float rampProgress = GetRampProgress();
            context.Velocity = slideForce * rampProgress + (1f - rampProgress * 0.5f) * context.Velocity;
        }
        else if (_phase == SlidingPhase.Full) {
            context.Velocity = slideForce + new Vector3(
                Config.MovementControlXZ * context.Velocity.x,
                Config.MovementControlY * context.Velocity.y - Config.YVelocityNudge,
                Config.MovementControlXZ * context.Velocity.z
            );

            CheckStuck(context.Position);
        }
    }

    private float GetRampProgress() {
        float elapsed = Time.time - _startTimestamp - Config.TimeToStart;
        return Mathf.Clamp01(elapsed / Config.TimeToFullSpeed);
    }

    private void CheckStuck(Vector3 currentPosition) {
        float movementDelta = (currentPosition - _lastPosition).magnitude;

        if (movementDelta < Config.StuckThreshold) {
            _stuckFrameCount++;
            if (_stuckFrameCount > Config.StoppedFrameThreshold) {
                EndSliding(true);
            }
        }
        else {
            _stuckFrameCount = 0;
        }
    }

    private void EndSliding(bool wasStuck) {
        _phase = SlidingPhase.None;
        _stuckFrameCount = 0;

        if (wasStuck) {
            Events.InvokeSlidingStuck();
        }
    }

    private void UpdateEvents(MovementContext context) {
        bool isCurrentlySliding = _phase != SlidingPhase.None;

        if (isCurrentlySliding && !_wasSlidingLastFrame) {
            SlidingEventData data = CreateEventData(context);
            Events.InvokeSlidingStarted(data);
        }
        else if (!isCurrentlySliding && _wasSlidingLastFrame) {
            Events.InvokeSlidingEnded();
        }
        else if (isCurrentlySliding) {
            SlidingEventData data = CreateEventData(context);
            Events.InvokeSlidingUpdate(data);
        }

        _wasSlidingLastFrame = isCurrentlySliding;
    }

    private SlidingEventData CreateEventData(MovementContext context) {
        return new SlidingEventData(
            context.GroundInfo.SlopeAngle,
            context.GroundInfo.SlopeNormal,
            context.GroundInfo.SlideDirection,
            context.Velocity.magnitude
        );
    }
}

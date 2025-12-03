using UnityEngine;

public class CrouchModifier : MovementModifierBase<CrouchConfig, CrouchEvents> {
    private bool _wasCrouchingLastFrame;

    public CrouchModifier(CrouchConfig config) : base(config) { }

    public override void ProcessMovement(ref MovementContext context) {
        bool wantsToCrouch = Input.CrouchPressed;

        // Check if we can stand up
        if (_wasCrouchingLastFrame && !wantsToCrouch) {
            if (!CanStandUp()) {
                wantsToCrouch = true;
                Events.InvokeCrouchBlocked();
            }
        }

        context.IsCrouching = wantsToCrouch;

        if (context.IsCrouching) {
            // TODO move slower
        }

        UpdateControllerHeight(context.IsCrouching);
        UpdateCameraPosition(context.IsCrouching, context.DeltaTime);

        if (context.IsCrouching != _wasCrouchingLastFrame) {
            Events.InvokeCrouchChanged(context.IsCrouching);
        }

        _wasCrouchingLastFrame = context.IsCrouching;
    }

    private bool CanStandUp() {
        float heightDifference = Config.StandingHeight - Config.CrouchingHeight;
        Vector3 origin = Controller.transform.position + Vector3.up * Config.CrouchingHeight;

        return !Physics.Raycast(origin, Vector3.up, heightDifference + 0.1f);
    }

    private void UpdateControllerHeight(bool isCrouching) {
        CharacterController cc = Controller.CharacterController;
        cc.height = isCrouching ? Config.CrouchingHeight : Config.StandingHeight;
        cc.center = new Vector3(0f, isCrouching ? Config.CrouchingCenterY : Config.StandingCenterY, 0f);
    }

    private void UpdateCameraPosition(bool isCrouching, float deltaTime) {
        Transform cameraHolder = Controller.CameraHolder;
        if (cameraHolder == null) {
            return;
        }

        float targetY = isCrouching ? Config.CameraCrouchingY : Config.CameraStandingY;
        Vector3 pos = cameraHolder.localPosition;
        pos.y = Mathf.MoveTowards(pos.y, targetY, Config.TransitionSpeed * deltaTime);
        cameraHolder.localPosition = pos;
    }
}

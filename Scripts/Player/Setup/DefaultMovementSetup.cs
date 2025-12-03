using UnityEngine;

[RequireComponent(typeof(FirstPersonController))]
public class DefaultMovementSetup : MonoBehaviour {
    [Header("Configs")]
    [SerializeField] private BaseMovementConfig baseMovementConfig;
    [SerializeField] private JumpConfig jumpConfig;
    [SerializeField] private RunConfig runConfig;
    [SerializeField] private CrouchConfig crouchConfig;
    [SerializeField] private LandingConfig landingConfig;
    [SerializeField] private SlidingConfig slidingConfig;

    [Header("Features")]
    [SerializeField] private bool enableCrouch = true;
    [SerializeField] private bool enableRun = true;
    [SerializeField] private bool enableSliding = true;

    private void Start() {
        FirstPersonController controller = GetComponent<FirstPersonController>();

        if (crouchConfig != null && enableCrouch) {
            controller.AddModifier(new CrouchModifier(crouchConfig));
        }

        if (runConfig != null && enableRun) {
            controller.AddModifier(new RunModifier(runConfig));
        }

        if (baseMovementConfig != null) {
            controller.AddModifier(new BaseMovementModifier(baseMovementConfig));
        }

        if (jumpConfig != null) {
            controller.AddModifier(new JumpModifier(jumpConfig));
        }

        if (landingConfig != null) {
            controller.AddModifier(new LandingModifier(landingConfig));
        }

        if (slidingConfig != null && enableSliding) {
            controller.AddModifier(new SlidingModifier(slidingConfig));
        }
    }
}

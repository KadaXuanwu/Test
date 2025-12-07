using KadaXuanwu.Utils.Runtime.FPController.Core.Interfaces;
using UnityEngine;

namespace KadaXuanwu.Utils.Runtime.FPController.Modifiers.Landing {
    [CreateAssetMenu(fileName = "LandingConfig", menuName = "Character/Modifiers/Landing")]
    public class LandingConfig : ScriptableObject, IModifierConfig {
        [Tooltip("Fall velocity at which landing causes full speed loss.")]
        [Min(1f)] public float VelocityDivisor = 15f;

        [Tooltip("Minimum fall velocity to trigger landing effects.")]
        [Min(0f)] public float MinImpactVelocity = 2f;

        public IMovementModifier CreateModifier() => new LandingModifier(this);
    }
}

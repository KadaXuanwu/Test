/// <summary>
/// Marker interface for modifier configurations.
/// </summary>
public interface IModifierConfig {
    IMovementModifier CreateModifier();
}

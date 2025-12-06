/// <summary>
/// Factory interface for creating character input handlers.
/// Implement this on a ScriptableObject to provide custom input handling.
/// </summary>
public interface ICharacterInputConfig {
    ICharacterInput CreateInput();
}

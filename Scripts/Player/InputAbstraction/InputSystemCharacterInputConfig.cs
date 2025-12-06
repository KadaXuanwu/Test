using UnityEngine;

[CreateAssetMenu(fileName = "InputSystemConfig", menuName = "Character/Input/Input System")]
public class InputSystemCharacterInputConfig : ScriptableObject, ICharacterInputConfig {
    public ICharacterInput CreateInput() {
        InputSystemCharacterInput input = new InputSystemCharacterInput();
        input.Initialize(InputManager.Instance.Actions.Player);
        return input;
    }
}

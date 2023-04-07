namespace Ryujinx.Input.Assigner
{
    /// <summary>
    /// <see cref="IButtonAssigner"/> implementation for <see cref="IKeyboard"/>.
    /// </summary>
    public class KeyboardKeyAssigner : IButtonAssigner
    {
        private IKeyboard _keyboard;

        private KeyboardStateSnapshot _keyboardState;

        public KeyboardKeyAssigner(IKeyboard keyboard)
        {
            _keyboard = keyboard;
        }

        public void Initialize() { }

        public void ReadInput()
        {
            _keyboardState = _keyboard.GetKeyboardStateSnapshot();
        }

        public bool HasAnyButtonPressed()
        {
            return GetPressedButton().Length != 0;
        }

        public bool ShouldCancel()
        {
            return _keyboardState.IsPressed(Key.Escape);
        }

        public string GetPressedButton()
        {
            string keyPressed = "";

            for (Key key = Key.Unknown; key < Key.Count; key++)
            {
                if (_keyboardState.IsPressed(key))
                {
                    keyPressed = key.ToString();
                    break;
                }
            }

            return !ShouldCancel() ? keyPressed : "";
        }
    }
}
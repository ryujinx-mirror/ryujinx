using OpenTK.Input;
using System;
using Key = Ryujinx.Configuration.Hid.Key;

namespace Ryujinx.Ui.Input
{
    class KeyboardKeyAssigner : ButtonAssigner
    {
        private int _index;

        private KeyboardState _keyboardState;

        public KeyboardKeyAssigner(int index)
        {
            _index = index;
        }

        public void Init() { }

        public void ReadInput()
        {
            _keyboardState = KeyboardController.GetKeyboardState(_index);
        }

        public bool HasAnyButtonPressed()
        {
            return _keyboardState.IsAnyKeyDown;
        }

        public bool ShouldCancel()
        {
            return Mouse.GetState().IsAnyButtonDown || Keyboard.GetState().IsKeyDown(OpenTK.Input.Key.Escape);
        }

        public string GetPressedButton()
        {
            string keyPressed = "";

            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                if (_keyboardState.IsKeyDown((OpenTK.Input.Key)key))
                {
                    keyPressed = key.ToString();
                    break;
                }
            }

            return !ShouldCancel() ? keyPressed : "";
        }
    }
}
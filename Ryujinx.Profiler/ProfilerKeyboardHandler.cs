using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Input;

namespace Ryujinx.Profiler
{
    public struct ProfilerButtons
    {
        public Key ToggleProfiler;
    }

    public class ProfilerKeyboardHandler
    {
        public ProfilerButtons Buttons;

        private KeyboardState _prevKeyboard;

        public ProfilerKeyboardHandler(ProfilerButtons buttons)
        {
            Buttons = buttons;
        }

        public bool TogglePressed(KeyboardState keyboard) => !keyboard[Buttons.ToggleProfiler] && _prevKeyboard[Buttons.ToggleProfiler];

        public void SetPrevKeyboardState(KeyboardState keyboard)
        {
            _prevKeyboard = keyboard;
        }
    }
}

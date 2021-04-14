using System;

namespace Ryujinx.Input.HLE
{
    public class InputManager : IDisposable
    {
        public IGamepadDriver KeyboardDriver { get; private set; }
        public IGamepadDriver GamepadDriver { get; private set; }

        public InputManager(IGamepadDriver keyboardDriver, IGamepadDriver gamepadDriver)
        {
            KeyboardDriver = keyboardDriver;
            GamepadDriver = gamepadDriver;
        }

        public NpadManager CreateNpadManager()
        {
            return new NpadManager(KeyboardDriver, GamepadDriver);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                KeyboardDriver?.Dispose();
                GamepadDriver?.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}

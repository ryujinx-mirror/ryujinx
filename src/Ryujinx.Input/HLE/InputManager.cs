using System;

namespace Ryujinx.Input.HLE
{
    public class InputManager : IDisposable
    {
        public IGamepadDriver KeyboardDriver { get; private set; }
        public IGamepadDriver GamepadDriver { get; private set; }
        public IGamepadDriver MouseDriver { get; private set; }

        public InputManager(IGamepadDriver keyboardDriver, IGamepadDriver gamepadDriver)
        {
            KeyboardDriver = keyboardDriver;
            GamepadDriver = gamepadDriver;
        }

        public void SetMouseDriver(IGamepadDriver mouseDriver)
        {
            MouseDriver?.Dispose();

            MouseDriver = mouseDriver;
        }

        public NpadManager CreateNpadManager()
        {
            return new NpadManager(KeyboardDriver, GamepadDriver, MouseDriver);
        }

        public TouchScreenManager CreateTouchScreenManager()
        {
            if (MouseDriver == null)
            {
                throw new InvalidOperationException("Mouse Driver has not been initialized.");
            }

            return new TouchScreenManager(MouseDriver.GetGamepad("0") as IMouse);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                KeyboardDriver?.Dispose();
                GamepadDriver?.Dispose();
                MouseDriver?.Dispose();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }
    }
}

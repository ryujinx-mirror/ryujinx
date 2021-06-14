using Ryujinx.HLE;
using Ryujinx.HLE.HOS.Services.Hid;
using System;

namespace Ryujinx.Input.HLE
{
    public class TouchScreenManager : IDisposable
    {
        private readonly IMouse _mouse;
        private Switch _device;

        public TouchScreenManager(IMouse mouse)
        {
            _mouse = mouse;
        }

        public void Initialize(Switch device)
        {
            _device = device;
        }

        public bool Update(bool isFocused, float aspectRatio = 0)
        {
            if (!isFocused)
            {
                _device.Hid.Touchscreen.Update();

                return false;
            }

            if (aspectRatio > 0)
            {
                var snapshot = IMouse.GetMouseStateSnapshot(_mouse);
                var touchPosition = IMouse.GetTouchPosition(snapshot.Position, _mouse.ClientSize, aspectRatio);

                TouchPoint currentPoint = new TouchPoint
                {
                    X = (uint)touchPosition.X,
                    Y = (uint)touchPosition.Y,

                    // Placeholder values till more data is acquired
                    DiameterX = 10,
                    DiameterY = 10,
                    Angle = 90
                };

                _device.Hid.Touchscreen.Update(currentPoint);

                return true;
            }

            return false;
        }

        public void Dispose() { }
    }
}
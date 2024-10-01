using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;
using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Mouse;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    public class MouseDevice : BaseDevice
    {
        public MouseDevice(Switch device, bool active) : base(device, active) { }

        public void Update(int mouseX, int mouseY, uint buttons = 0, int scrollX = 0, int scrollY = 0, bool connected = false)
        {
            ref RingLifo<MouseState> lifo = ref _device.Hid.SharedMemory.Mouse;

            ref MouseState previousEntry = ref lifo.GetCurrentEntryRef();

            MouseState newState = new()
            {
                SamplingNumber = previousEntry.SamplingNumber + 1,
            };

            if (Active)
            {
                newState.Buttons = (MouseButton)buttons;
                newState.X = mouseX;
                newState.Y = mouseY;
                newState.DeltaX = mouseX - previousEntry.X;
                newState.DeltaY = mouseY - previousEntry.Y;
                newState.WheelDeltaX = scrollX;
                newState.WheelDeltaY = scrollY;
                newState.Attributes = connected ? MouseAttribute.IsConnected : MouseAttribute.None;
            }

            lifo.Write(ref newState);
        }
    }
}

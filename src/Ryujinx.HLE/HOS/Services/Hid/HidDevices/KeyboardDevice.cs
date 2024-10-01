using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;
using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Keyboard;
using System;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    public class KeyboardDevice : BaseDevice
    {
        public KeyboardDevice(Switch device, bool active) : base(device, active) { }

        public void Update(KeyboardInput keyState)
        {
            ref RingLifo<KeyboardState> lifo = ref _device.Hid.SharedMemory.Keyboard;

            if (!Active)
            {
                lifo.Clear();

                return;
            }

            ref KeyboardState previousEntry = ref lifo.GetCurrentEntryRef();

            KeyboardState newState = new()
            {
                SamplingNumber = previousEntry.SamplingNumber + 1,
            };

            keyState.Keys.AsSpan().CopyTo(newState.Keys.RawData.AsSpan());
            newState.Modifiers = (KeyboardModifier)keyState.Modifier;

            lifo.Write(ref newState);
        }
    }
}

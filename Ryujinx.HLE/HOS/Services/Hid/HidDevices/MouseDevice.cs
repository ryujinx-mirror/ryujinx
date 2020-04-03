namespace Ryujinx.HLE.HOS.Services.Hid
{
    public class MouseDevice : BaseDevice
    {
        public MouseDevice(Switch device, bool active) : base(device, active) { }

        public void Update(int mouseX, int mouseY, int buttons = 0, int scrollX = 0, int scrollY = 0)
        {
            ref ShMemMouse mouse = ref _device.Hid.SharedMemory.Mouse;

            int currentIndex = UpdateEntriesHeader(ref mouse.Header, out int previousIndex);

            if (!Active)
            {
                return;
            }

            ref MouseState currentEntry = ref mouse.Entries[currentIndex];
            MouseState previousEntry = mouse.Entries[previousIndex];

            currentEntry.SampleTimestamp = previousEntry.SampleTimestamp + 1;
            currentEntry.SampleTimestamp2 = previousEntry.SampleTimestamp2 + 1;

            currentEntry.Buttons = (ulong)buttons;

            currentEntry.Position = new MousePosition
            {
                X = mouseX,
                Y = mouseY,
                VelocityX = mouseX - previousEntry.Position.X,
                VelocityY = mouseY - previousEntry.Position.Y,
                ScrollVelocityX = scrollX,
                ScrollVelocityY = scrollY
            };
        }
    }
}
namespace Ryujinx.HLE.HOS.Services.Hid
{
    public class DebugPadDevice : BaseDevice
    {
        public DebugPadDevice(Switch device, bool active) : base(device, active) { }

        public void Update()
        {
            ref ShMemDebugPad debugPad = ref _device.Hid.SharedMemory.DebugPad;

            int currentIndex = UpdateEntriesHeader(ref debugPad.Header, out int previousIndex);

            if (!Active)
            {
                return;
            }

            ref DebugPadEntry currentEntry = ref debugPad.Entries[currentIndex];
            DebugPadEntry previousEntry = debugPad.Entries[previousIndex];

            currentEntry.SampleTimestamp = previousEntry.SampleTimestamp + 1;
            currentEntry.SampleTimestamp2 = previousEntry.SampleTimestamp2 + 1;
        }
    }
}
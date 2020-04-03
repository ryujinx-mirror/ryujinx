using static Ryujinx.HLE.HOS.Services.Hid.Hid;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    public abstract class BaseDevice
    {
        protected readonly Switch _device;
        public bool Active;

        public BaseDevice(Switch device, bool active)
        {
            _device = device;
            Active = active;
        }

        internal static int UpdateEntriesHeader(ref CommonEntriesHeader header, out int previousEntry)
        {
            header.NumEntries = SharedMemEntryCount;
            header.MaxEntryIndex = SharedMemEntryCount - 1;

            previousEntry = (int)header.LatestEntry;
            header.LatestEntry = (header.LatestEntry + 1) % SharedMemEntryCount;

            header.TimestampTicks = GetTimestampTicks();

            return (int)header.LatestEntry; // EntryCount shouldn't overflow int
        }
    }
}
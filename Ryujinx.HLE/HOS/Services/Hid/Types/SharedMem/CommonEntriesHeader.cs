namespace Ryujinx.HLE.HOS.Services.Hid
{
    struct CommonEntriesHeader
    {
        public ulong TimestampTicks;
        public ulong NumEntries;
        public ulong LatestEntry;
        public ulong MaxEntryIndex;
    }
}


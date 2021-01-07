namespace Ryujinx.HLE.HOS.Services.Hid
{
    unsafe struct DebugPadEntry
    {
        public ulong SampleTimestamp;
        public ulong SampleTimestamp2;
        fixed byte _unknown[0x18];
    }
}
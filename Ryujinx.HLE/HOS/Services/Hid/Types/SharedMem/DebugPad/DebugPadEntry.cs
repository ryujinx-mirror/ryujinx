namespace Ryujinx.HLE.HOS.Services.Hid
{
    unsafe struct DebugPadEntry
    {
        public ulong SampleTimestamp;
        fixed byte _unknown[0x20];
    }
}
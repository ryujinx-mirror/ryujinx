namespace Ryujinx.HLE.HOS.Services.Hid
{
    unsafe struct KeyboardState
    {
        public ulong SampleTimestamp;
        public ulong SampleTimestamp2;
        public ulong Modifier;
        public fixed uint Keys[8];
    }
}
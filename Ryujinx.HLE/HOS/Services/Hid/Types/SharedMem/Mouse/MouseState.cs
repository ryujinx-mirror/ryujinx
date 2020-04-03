namespace Ryujinx.HLE.HOS.Services.Hid
{
    struct MouseState
    {
        public ulong SampleTimestamp;
        public ulong SampleTimestamp2;
        public MousePosition Position;
        public ulong Buttons;
    }
}
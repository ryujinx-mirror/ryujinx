namespace Ryujinx.HLE.HOS.Services.Hid
{
    struct NpadState
    {
        public ulong SampleTimestamp;
        public ulong SampleTimestamp2;
        public ControllerKeys Buttons;
        public int LStickX;
        public int LStickY;
        public int RStickX;
        public int RStickY;
        public NpadConnectionState ConnectionState;
    }
}
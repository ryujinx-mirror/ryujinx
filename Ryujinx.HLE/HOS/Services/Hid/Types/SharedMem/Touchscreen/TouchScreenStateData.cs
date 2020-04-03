namespace Ryujinx.HLE.HOS.Services.Hid
{
    struct TouchScreenStateData
    {
        public ulong SampleTimestamp;
        uint _padding;
        public uint TouchIndex;
        public uint X;
        public uint Y;
        public uint DiameterX;
        public uint DiameterY;
        public uint Angle;
        uint _padding2;
    }
}
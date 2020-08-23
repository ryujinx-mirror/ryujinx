namespace Ryujinx.HLE.HOS.Services.Hid
{
    struct TouchScreenStateData
    {
        public ulong SampleTimestamp;
#pragma warning disable CS0169
        uint _padding;
#pragma warning restore CS0169
        public uint TouchIndex;
        public uint X;
        public uint Y;
        public uint DiameterX;
        public uint DiameterY;
        public uint Angle;
#pragma warning disable CS0169
        uint _padding2;
#pragma warning restore CS0169
    }
}
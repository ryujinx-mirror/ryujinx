namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.TouchScreen
{
    struct TouchState
    {
        public ulong DeltaTime;
#pragma warning disable CS0649
        public TouchAttribute Attribute;
#pragma warning restore CS0649
        public uint FingerId;
        public uint X;
        public uint Y;
        public uint DiameterX;
        public uint DiameterY;
        public uint RotationAngle;
#pragma warning disable CS0169
        private uint _reserved;
#pragma warning restore CS0169
    }
}

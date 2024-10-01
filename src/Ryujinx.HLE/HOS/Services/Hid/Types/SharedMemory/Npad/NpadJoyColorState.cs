namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Npad
{
    struct NpadJoyColorState
    {
        public NpadColorAttribute Attribute;
        public uint LeftBody;
        public uint LeftButtons;
        public uint RightBody;
        public uint RightButtons;
    }
}

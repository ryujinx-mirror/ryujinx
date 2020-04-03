namespace Ryujinx.HLE.HOS.Services.Hid
{
    struct NpadStateHeader
    {
        public ControllerType Type;
        public Boolean32 IsHalf;
        public NpadColorDescription SingleColorsDescriptor;
        public NpadColor SingleColorBody;
        public NpadColor SingleColorButtons;
        public NpadColorDescription SplitColorsDescriptor;
        public NpadColor LeftColorBody;
        public NpadColor LeftColorButtons;
        public NpadColor RightColorBody;
        public NpadColor RightColorButtons;
    }
}
namespace Ryujinx.HLE.HOS.Applets
{
    struct ControllerSupportArgHeader
    {
        public sbyte PlayerCountMin;
        public sbyte PlayerCountMax;
        public byte EnableTakeOverConnection;
        public byte EnableLeftJustify;
        public byte EnablePermitJoyDual;
        public byte EnableSingleMode;
        public byte EnableIdentificationColor;
    }
}
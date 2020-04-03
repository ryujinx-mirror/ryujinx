namespace Ryujinx.HLE.HOS.Applets
{
    struct ControllerSupportArgPrivate
    {
        public uint PrivateSize;
        public uint ArgSize;
        public byte Flag0;
        public byte Flag1;
        public ControllerSupportMode Mode;
        public byte ControllerSupportCaller;
        public uint NpadStyleSet;
        public uint NpadJoyHoldType;
    }
}
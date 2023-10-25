namespace Ryujinx.HLE.HOS.Services.Ldn
{
    static class LdnConst
    {
        public const int SsidLengthMax = 0x20;
        public const int AdvertiseDataSizeMax = 0x180;
        public const int UserNameBytesMax = 0x20;
        public const int NodeCountMax = 8;
        public const int StationCountMax = NodeCountMax - 1;
        public const int PassphraseLengthMax = 0x40;
    }
}

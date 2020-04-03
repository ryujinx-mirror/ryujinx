namespace Ryujinx.HLE.HOS.Services.Hid
{
    struct NpadSixAxis
    {
        public CommonEntriesHeader Header;
        public Array17<SixAxisState> Entries;
    }
}
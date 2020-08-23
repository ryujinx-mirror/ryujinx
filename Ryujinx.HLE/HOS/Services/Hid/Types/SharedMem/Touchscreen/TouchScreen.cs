using Ryujinx.Common.Memory;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    unsafe struct ShMemTouchScreen
    {
        public CommonEntriesHeader Header;
        public Array17<TouchScreenState> Entries;
        fixed byte _padding[0x3c8];
    }
}
using Ryujinx.Common.Memory;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    struct NpadLayout
    {
        public CommonEntriesHeader Header;
        public Array17<NpadState> Entries;
    }
}
using Ryujinx.Common.Memory;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    unsafe struct ShMemDebugPad
    {
        public CommonEntriesHeader Header;
        public Array17<DebugPadEntry> Entries;
        fixed byte _padding[0x138];
    }
}
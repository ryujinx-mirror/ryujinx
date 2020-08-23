using Ryujinx.Common.Memory;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    unsafe struct ShMemKeyboard
    {
        public CommonEntriesHeader Header;
        public Array17<KeyboardState> Entries;
        fixed byte _padding[0x28];
    }
}
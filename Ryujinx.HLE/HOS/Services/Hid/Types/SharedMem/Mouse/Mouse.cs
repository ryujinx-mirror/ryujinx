
using Ryujinx.Common.Memory;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    unsafe struct ShMemMouse
    {
        public CommonEntriesHeader Header;
        public Array17<MouseState> Entries;
        fixed byte _padding[0xB0];
    }
}
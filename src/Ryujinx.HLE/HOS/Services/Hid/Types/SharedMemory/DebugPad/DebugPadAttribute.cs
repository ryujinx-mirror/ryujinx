using System;

namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.DebugPad
{
    [Flags]
    enum DebugPadAttribute : uint
    {
        None = 0,
        Connected = 1 << 0,
    }
}

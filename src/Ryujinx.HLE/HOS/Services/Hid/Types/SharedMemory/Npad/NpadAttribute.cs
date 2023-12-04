using System;

namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Npad
{
    [Flags]
    enum NpadAttribute : uint
    {
        None = 0,
        IsConnected = 1 << 0,
        IsWired = 1 << 1,
        IsLeftConnected = 1 << 2,
        IsLeftWired = 1 << 3,
        IsRightConnected = 1 << 4,
        IsRightWired = 1 << 5,
    }
}

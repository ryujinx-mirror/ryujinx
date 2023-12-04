using System;

namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Npad
{
    [Flags]
    enum SixAxisSensorAttribute : uint
    {
        None = 0,
        IsConnected = 1 << 0,
        IsInterpolated = 1 << 1,
    }
}

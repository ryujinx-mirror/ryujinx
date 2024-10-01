using System;

namespace Ryujinx.Horizon.Sdk.Lm
{
    [Flags]
    enum LogDestination
    {
        TargetManager = 1 << 0,
        Uart = 1 << 1,
        UartIfSleep = 1 << 2,

        All = 0xffff,
    }
}

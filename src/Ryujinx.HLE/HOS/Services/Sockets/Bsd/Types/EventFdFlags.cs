using System;

namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd.Types
{
    [Flags]
    enum EventFdFlags : uint
    {
        None = 0,
        Semaphore = 1 << 0,
        NonBlocking = 1 << 2,
    }
}

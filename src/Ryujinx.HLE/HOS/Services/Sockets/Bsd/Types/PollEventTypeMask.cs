using System;

namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd.Types
{
    [Flags]
    enum PollEventTypeMask : ushort
    {
        Input = 1,
        UrgentInput = 2,
        Output = 4,
        Error = 8,
        Disconnected = 0x10,
        Invalid = 0x20,
    }
}

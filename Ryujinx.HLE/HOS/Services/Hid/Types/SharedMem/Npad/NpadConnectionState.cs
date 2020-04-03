using System;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    [Flags]
    enum NpadConnectionState : long
    {
        ControllerStateConnected = (1 << 0),
        ControllerStateWired = (1 << 1),
        JoyLeftConnected = (1 << 2),
        JoyRightConnected = (1 << 4)
    }
}
using System;

namespace Ryujinx.HLE.Input
{
    [Flags]
    public enum ControllerConnectionState : long
    {
        ControllerStateConnected = (1 << 0),
        ControllerStateWired     = (1 << 1)
    }
}
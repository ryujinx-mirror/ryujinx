using System;

namespace Ryujinx.HLE.Input
{
    [Flags]
    public enum HidControllerConnState
    {
        ControllerStateConnected = (1 << 0),
        ControllerStateWired     = (1 << 1)
    }
}
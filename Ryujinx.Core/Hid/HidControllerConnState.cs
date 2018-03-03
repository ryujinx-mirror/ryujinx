using System;

namespace Ryujinx.Core.Input
{
    [Flags]
    public enum HidControllerConnState
    {
        Controller_State_Connected = (1 << 0),
        Controller_State_Wired     = (1 << 1)
    }
}
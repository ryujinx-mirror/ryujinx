using System;

namespace Ryujinx.HLE.Input
{
    [Flags]
    public enum HidControllerType
    {
        ControllerType_ProController = (1 << 0),
        ControllerType_Handheld      = (1 << 1),
        ControllerType_JoyconPair    = (1 << 2),
        ControllerType_JoyconLeft    = (1 << 3),
        ControllerType_JoyconRight   = (1 << 4)
    }
}
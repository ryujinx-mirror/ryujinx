using System;

namespace Ryujinx.HLE.Input
{
    [Flags]
    public enum ControllerDeviceType : int
    {
        ProController       = 1 << 0,
        NPadLeftController  = 1 << 4,
        NPadRightController = 1 << 5,
    }
}

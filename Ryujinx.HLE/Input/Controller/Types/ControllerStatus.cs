using System;

namespace Ryujinx.HLE.Input
{
    [Flags]
    public enum ControllerStatus : int
    {
        ProController = 1 << 0,
        Handheld      = 1 << 1,
        NpadPair      = 1 << 2,
        NpadLeft      = 1 << 3,
        NpadRight     = 1 << 4
    }
}
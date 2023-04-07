using System;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    [Flags]
    public enum ControllerType : int
    {
        None,
        ProController  = 1 << 0,
        Handheld       = 1 << 1,
        JoyconPair     = 1 << 2,
        JoyconLeft     = 1 << 3,
        JoyconRight    = 1 << 4,
        Invalid        = 1 << 5,
        Pokeball       = 1 << 6,
        SystemExternal = 1 << 29,
        System         = 1 << 30
    }
}
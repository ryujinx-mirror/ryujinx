using System;

namespace Ryujinx.Common.Configuration.Hid
{
    [Flags]
    // This enum was duplicated from Ryujinx.HLE.HOS.Services.Hid.PlayerIndex and should be kept identical
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
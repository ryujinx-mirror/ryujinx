using System;

namespace Ryujinx.HLE.Input
{
    [Flags]
    public enum HidControllerButtons
    {
        A           = 1 << 0,
        B           = 1 << 1,
        X           = 1 << 2,
        Y           = 1 << 3,
        StickLeft   = 1 << 4,
        StickRight  = 1 << 5,
        L           = 1 << 6,
        R           = 1 << 7,
        Zl          = 1 << 8,
        Zr          = 1 << 9,
        Plus        = 1 << 10,
        Minus       = 1 << 11,
        DpadLeft    = 1 << 12,
        DpadUp      = 1 << 13,
        DPadRight   = 1 << 14,
        DpadDown    = 1 << 15,
        LStickLeft  = 1 << 16,
        LStickUp    = 1 << 17,
        LStickRight = 1 << 18,
        LStickDown  = 1 << 19,
        RStickLeft  = 1 << 20,
        RStickUp    = 1 << 21,
        RStickRight = 1 << 22,
        RStickDown  = 1 << 23,
        Sl          = 1 << 24,
        Sr          = 1 << 25
    }
}
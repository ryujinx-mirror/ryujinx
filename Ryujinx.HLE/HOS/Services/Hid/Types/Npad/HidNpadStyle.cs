using System;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    [Flags]
    public enum HidNpadStyle
    {
        None,
        FullKey  = 1 << 0,
        Handheld = 1 << 1,
        Dual     = 1 << 2,
        Left     = 1 << 3,
        Right    = 1 << 4,
        Invalid  = 1 << 5
    }
}
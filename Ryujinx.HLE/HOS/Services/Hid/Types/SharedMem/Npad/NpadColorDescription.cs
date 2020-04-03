using System;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    [Flags]
    enum NpadColorDescription : int
    {
        ColorDescriptionColorsNonexistent = (1 << 1)
    }
}
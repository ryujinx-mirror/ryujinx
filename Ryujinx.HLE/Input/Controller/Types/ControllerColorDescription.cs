using System;

namespace Ryujinx.HLE.Input
{
    [Flags]
    public enum ControllerColorDescription : int
    {
        ColorDescriptionColorsNonexistent = (1 << 1)
    }
}
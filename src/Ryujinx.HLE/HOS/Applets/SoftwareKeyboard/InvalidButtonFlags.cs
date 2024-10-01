using System;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// Identifies prohibited buttons.
    /// </summary>
    [Flags]
    enum InvalidButtonFlags : uint
    {
        None = 0,
        AnalogStickL = 1 << 1,
        AnalogStickR = 1 << 2,
        ZL = 1 << 3,
        ZR = 1 << 4,
    }
}

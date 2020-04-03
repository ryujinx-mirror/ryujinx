using System;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    [Flags]
    enum NpadSystemProperties : long
    {
        PowerInfo0Charging = 1 << 0,
        PowerInfo1Charging = 1 << 1,
        PowerInfo2Charging = 1 << 2,
        PowerInfo0Connected = 1 << 3,
        PowerInfo1Connected = 1 << 4,
        PowerInfo2Connected = 1 << 5,
        UnsupportedButtonPressedNpadSystem = 1 << 9,
        UnsupportedButtonPressedNpadSystemExt = 1 << 10,
        AbxyButtonOriented = 1 << 11,
        SlSrButtonOriented = 1 << 12,
        PlusButtonCapability = 1 << 13,
        MinusButtonCapability = 1 << 14,
        DirectionalButtonsSupported = 1 << 15
    }
}
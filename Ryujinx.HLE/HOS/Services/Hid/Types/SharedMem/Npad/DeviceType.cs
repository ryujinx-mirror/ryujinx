using System;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    [Flags]
    enum DeviceType : int
    {
        FullKey = 1 << 0,
        DebugPad = 1 << 1,
        HandheldLeft = 1 << 2,
        HandheldRight = 1 << 3,
        JoyLeft = 1 << 4,
        JoyRight = 1 << 5,
        Palma = 1 << 6, // Poké Ball Plus
        FamicomLeft = 1 << 7,
        FamicomRight = 1 << 8,
        NESLeft = 1 << 9,
        NESRight = 1 << 10,
        HandheldFamicomLeft = 1 << 11,
        HandheldFamicomRight = 1 << 12,
        HandheldNESLeft = 1 << 13,
        HandheldNESRight = 1 << 14,
        Lucia = 1 << 15,
        System = 1 << 31
    }
}
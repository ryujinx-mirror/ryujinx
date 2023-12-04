using System;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostAsGpu.Types
{
    [Flags]
    enum AddressSpaceFlags : uint
    {
        FixedOffset = 1,
        RemapSubRange = 0x100,
    }
}

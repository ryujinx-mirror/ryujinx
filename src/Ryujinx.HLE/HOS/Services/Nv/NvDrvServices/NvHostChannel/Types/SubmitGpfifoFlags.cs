using System;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostChannel.Types
{
    [Flags]
    enum SubmitGpfifoFlags : uint
    {
        None,
        FenceWait          = 1 << 0,
        FenceIncrement     = 1 << 1,
        HwFormat           = 1 << 2,
        SuppressWfi        = 1 << 4,
        IncrementWithValue = 1 << 8,
    }
}

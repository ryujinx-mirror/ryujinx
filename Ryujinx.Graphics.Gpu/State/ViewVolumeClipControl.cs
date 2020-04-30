using System;

namespace Ryujinx.Graphics.Gpu.State
{
    [Flags]
    enum ViewVolumeClipControl
    {
        ForceDepthRangeZeroToOne = 1 << 0,
        DepthClampDisabled       = 1 << 11,
    }
}
using System;

namespace Ryujinx.Graphics.Gpu.State
{
    [Flags]
    enum ViewVolumeClipControl
    {
        ForceDepthRangeZeroToOne = 1 << 0,
        DepthClampNear           = 1 << 3,
        DepthClampFar            = 1 << 4,
    }
}
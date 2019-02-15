using System;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Vi
{
    enum SrcScalingMode
    {
        Freeze = 0,
        ScaleToWindow = 1,
        ScaleAndCrop = 2,
        None = 3,
        PreserveAspectRatio = 4
    }

    enum DstScalingMode
    {
        None = 0,
        Freeze = 1,
        ScaleToWindow = 2,
        ScaleAndCrop = 3,
        PreserveAspectRatio = 4
    }
}

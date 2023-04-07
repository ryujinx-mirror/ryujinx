using System;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    [Flags]
    enum NativeWindowTransform : uint
    {
        None              = 0,
        FlipX             = 1,
        FlipY             = 2,
        Rotate90          = 4,
        Rotate180         = FlipX | FlipY,
        Rotate270         = Rotate90 | Rotate180,
        InverseDisplay    = 8,
        NoVSyncCapability = 0x10,
        ReturnFrameNumber = 0x20
    }
}

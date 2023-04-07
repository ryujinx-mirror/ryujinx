using System;

namespace Ryujinx.HLE.HOS.Services.Audio.Types
{
    [Flags]
    enum OpusDecoderFlags : uint
    {
        None,
        LargeFrameSize = 1 << 0,
    }
}

using System;

namespace Ryujinx.Graphics.Gpu.Image
{
    [Flags]
    enum TextureSearchFlags
    {
        None     = 0,
        IgnoreMs = 1 << 0,
        Strict   = 1 << 1 | Sampler,
        Sampler  = 1 << 2
    }
}
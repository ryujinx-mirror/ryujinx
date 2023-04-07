using System;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Texture search flags, defines texture information comparison rules.
    /// </summary>
    [Flags]
    enum TextureSearchFlags
    {
        None        = 0,
        ForSampler  = 1 << 1,
        ForCopy     = 1 << 2,
        WithUpscale = 1 << 3,
        NoCreate    = 1 << 4
    }
}
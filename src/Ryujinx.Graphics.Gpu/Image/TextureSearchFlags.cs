using System;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Texture search flags, defines texture information comparison rules.
    /// </summary>
    [Flags]
    enum TextureSearchFlags
    {
        None = 0,
        ForSampler = 1 << 1,
        ForCopy = 1 << 2,
        DepthAlias = 1 << 3,
        WithUpscale = 1 << 4,
        NoCreate = 1 << 5,
        DiscardData = 1 << 6,
    }
}

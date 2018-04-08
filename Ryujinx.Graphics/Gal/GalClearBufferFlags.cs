using System;

namespace Ryujinx.Graphics.Gal
{
    [Flags]
    public enum GalClearBufferFlags
    {
        Depth      = 1 << 0,
        Stencil    = 1 << 1,
        ColorRed   = 1 << 2,
        ColorGreen = 1 << 3,
        ColorBlue  = 1 << 4,
        ColorAlpha = 1 << 5
    }
}
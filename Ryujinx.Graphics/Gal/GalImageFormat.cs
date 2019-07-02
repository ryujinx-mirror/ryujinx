// ReSharper disable InconsistentNaming
using System;

namespace Ryujinx.Graphics.Gal
{
    [Flags]
    public enum GalImageFormat
    {
        Astc2DStart,
        Astc2D4x4,
        Astc2D5x4,
        Astc2D5x5,
        Astc2D6x5,
        Astc2D6x6,
        Astc2D8x5,
        Astc2D8x6,
        Astc2D8x8,
        Astc2D10x5,
        Astc2D10x6,
        Astc2D10x8,
        Astc2D10x10,
        Astc2D12x10,
        Astc2D12x12,
        Astc2DEnd,

        Rgba4,
        Rgb565,
        Bgr565,
        Bgr5A1,
        Rgb5A1,
        R8,
        Rg8,
        Rgbx8,
        Rgba8,
        Bgra8,
        Rgb10A2,
        R16,
        Rg16,
        Rgba16,
        R32,
        Rg32,
        Rgba32,
        R11G11B10,
        D16,
        D24,
        D32,
        D24S8,
        D32S8,
        BC1,
        BC2,
        BC3,
        BC4,
        BC5,
        BptcSfloat,
        BptcUfloat,
        BptcUnorm,

        Snorm = 1 << 26,
        Unorm = 1 << 27,
        Sint  = 1 << 28,
        Uint  = 1 << 39,
        Float = 1 << 30,
        Srgb  = 1 << 31,

        TypeMask = Snorm | Unorm | Sint | Uint | Float | Srgb,

        FormatMask = ~TypeMask
    }
}
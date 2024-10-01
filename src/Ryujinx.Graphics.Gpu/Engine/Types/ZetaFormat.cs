using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;

namespace Ryujinx.Graphics.Gpu.Engine.Types
{
    /// <summary>
    /// Depth-stencil texture format.
    /// </summary>
    enum ZetaFormat
    {
        Zf32 = 0xa,
        Z16 = 0x13,
        Z24S8 = 0x14,
        X8Z24 = 0x15,
        S8Z24 = 0x16,
        S8Uint = 0x17,
        Zf32X24S8 = 0x19,
    }

    static class ZetaFormatConverter
    {
        /// <summary>
        /// Converts the depth-stencil texture format to a host compatible format.
        /// </summary>
        /// <param name="format">Depth-stencil format</param>
        /// <returns>Host compatible format enum value</returns>
        public static FormatInfo Convert(this ZetaFormat format)
        {
            return format switch
            {
#pragma warning disable IDE0055 // Disable formatting
                ZetaFormat.Zf32      => new FormatInfo(Format.D32Float,       1, 1, 4, 1),
                ZetaFormat.Z16       => new FormatInfo(Format.D16Unorm,       1, 1, 2, 1),
                ZetaFormat.Z24S8     => new FormatInfo(Format.D24UnormS8Uint, 1, 1, 4, 2),
                ZetaFormat.X8Z24     => new FormatInfo(Format.X8UintD24Unorm, 1, 1, 4, 1),
                ZetaFormat.S8Z24     => new FormatInfo(Format.S8UintD24Unorm, 1, 1, 4, 2),
                ZetaFormat.S8Uint    => new FormatInfo(Format.S8Uint,         1, 1, 1, 1),
                ZetaFormat.Zf32X24S8 => new FormatInfo(Format.D32FloatS8Uint, 1, 1, 8, 2),
                _                    => FormatInfo.Default,
#pragma warning restore IDE0055
            };
        }
    }
}

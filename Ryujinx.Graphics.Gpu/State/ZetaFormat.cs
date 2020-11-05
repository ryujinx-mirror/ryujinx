using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;

namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Depth-stencil texture format.
    /// </summary>
    enum ZetaFormat
    {
        D32Float       = 0xa,
        D16Unorm       = 0x13,
        D24UnormS8Uint = 0x14,
        D24Unorm       = 0x15,
        S8UintD24Unorm = 0x16,
        S8Uint         = 0x17,
        D32FloatS8Uint = 0x19
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
                ZetaFormat.D32Float          => new FormatInfo(Format.D32Float,          1, 1, 4,  1),
                ZetaFormat.D16Unorm          => new FormatInfo(Format.D16Unorm,          1, 1, 2,  1),
                ZetaFormat.D24UnormS8Uint    => new FormatInfo(Format.D24UnormS8Uint,    1, 1, 4,  2),
                ZetaFormat.D24Unorm          => new FormatInfo(Format.D24UnormS8Uint,    1, 1, 4,  1),
                ZetaFormat.S8UintD24Unorm    => new FormatInfo(Format.D24UnormS8Uint,    1, 1, 4,  2),
                ZetaFormat.S8Uint            => new FormatInfo(Format.S8Uint,            1, 1, 1,  1),
                ZetaFormat.D32FloatS8Uint    => new FormatInfo(Format.D32FloatS8Uint,    1, 1, 8,  2),
                _                            => FormatInfo.Default
            };
        }
    }
}

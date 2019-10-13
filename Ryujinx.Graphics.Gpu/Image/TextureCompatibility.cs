using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.Image
{
    static class TextureCompatibility
    {
        private enum FormatClass
        {
            Unclassified,
            BCn64,
            BCn128,
            Bc1Rgb,
            Bc1Rgba,
            Bc2,
            Bc3,
            Bc4,
            Bc5,
            Bc6,
            Bc7
        }

        public static bool FormatCompatible(FormatInfo lhs, FormatInfo rhs)
        {
            if (IsDsFormat(lhs.Format) || IsDsFormat(rhs.Format))
            {
                return lhs.Format == rhs.Format;
            }

            if (lhs.Format.IsAstc() || rhs.Format.IsAstc())
            {
                return lhs.Format == rhs.Format;
            }

            if (lhs.IsCompressed && rhs.IsCompressed)
            {
                FormatClass lhsClass = GetFormatClass(lhs.Format);
                FormatClass rhsClass = GetFormatClass(rhs.Format);

                return lhsClass == rhsClass;
            }
            else
            {
                return lhs.BytesPerPixel == rhs.BytesPerPixel;
            }
        }

        private static FormatClass GetFormatClass(Format format)
        {
            switch (format)
            {
                case Format.Bc1RgbSrgb:
                case Format.Bc1RgbUnorm:
                    return FormatClass.Bc1Rgb;
                case Format.Bc1RgbaSrgb:
                case Format.Bc1RgbaUnorm:
                    return FormatClass.Bc1Rgba;
                case Format.Bc2Srgb:
                case Format.Bc2Unorm:
                    return FormatClass.Bc2;
                case Format.Bc3Srgb:
                case Format.Bc3Unorm:
                    return FormatClass.Bc3;
                case Format.Bc4Snorm:
                case Format.Bc4Unorm:
                    return FormatClass.Bc4;
                case Format.Bc5Snorm:
                case Format.Bc5Unorm:
                    return FormatClass.Bc5;
                case Format.Bc6HSfloat:
                case Format.Bc6HUfloat:
                    return FormatClass.Bc6;
                case Format.Bc7Srgb:
                case Format.Bc7Unorm:
                    return FormatClass.Bc7;
            }

            return FormatClass.Unclassified;
        }

        private static bool IsDsFormat(Format format)
        {
            switch (format)
            {
                case Format.D16Unorm:
                case Format.D24X8Unorm:
                case Format.D24UnormS8Uint:
                case Format.D32Float:
                case Format.D32FloatS8Uint:
                    return true;
            }

            return false;
        }
    }
}
namespace Ryujinx.Graphics.GAL
{
    public enum Format
    {
        R8Unorm,
        R8Snorm,
        R8Uint,
        R8Sint,
        R16Float,
        R16Unorm,
        R16Snorm,
        R16Uint,
        R16Sint,
        R32Float,
        R32Uint,
        R32Sint,
        R8G8Unorm,
        R8G8Snorm,
        R8G8Uint,
        R8G8Sint,
        R16G16Float,
        R16G16Unorm,
        R16G16Snorm,
        R16G16Uint,
        R16G16Sint,
        R32G32Float,
        R32G32Uint,
        R32G32Sint,
        R8G8B8Unorm,
        R8G8B8Snorm,
        R8G8B8Uint,
        R8G8B8Sint,
        R16G16B16Float,
        R16G16B16Unorm,
        R16G16B16Snorm,
        R16G16B16Uint,
        R16G16B16Sint,
        R32G32B32Float,
        R32G32B32Uint,
        R32G32B32Sint,
        R8G8B8A8Unorm,
        R8G8B8A8Snorm,
        R8G8B8A8Uint,
        R8G8B8A8Sint,
        R16G16B16A16Float,
        R16G16B16A16Unorm,
        R16G16B16A16Snorm,
        R16G16B16A16Uint,
        R16G16B16A16Sint,
        R32G32B32A32Float,
        R32G32B32A32Uint,
        R32G32B32A32Sint,
        S8Uint,
        D16Unorm,
        D24X8Unorm,
        D32Float,
        D24UnormS8Uint,
        D32FloatS8Uint,
        R8G8B8X8Srgb,
        R8G8B8A8Srgb,
        R4G4B4A4Unorm,
        R5G5B5X1Unorm,
        R5G5B5A1Unorm,
        R5G6B5Unorm,
        R10G10B10A2Unorm,
        R10G10B10A2Uint,
        R11G11B10Float,
        R9G9B9E5Float,
        Bc1RgbUnorm,
        Bc1RgbaUnorm,
        Bc2Unorm,
        Bc3Unorm,
        Bc1RgbSrgb,
        Bc1RgbaSrgb,
        Bc2Srgb,
        Bc3Srgb,
        Bc4Unorm,
        Bc4Snorm,
        Bc5Unorm,
        Bc5Snorm,
        Bc7Unorm,
        Bc7Srgb,
        Bc6HUfloat,
        Bc6HSfloat,
        R8Uscaled,
        R8Sscaled,
        R16Uscaled,
        R16Sscaled,
        R32Uscaled,
        R32Sscaled,
        R8G8Uscaled,
        R8G8Sscaled,
        R16G16Uscaled,
        R16G16Sscaled,
        R32G32Uscaled,
        R32G32Sscaled,
        R8G8B8Uscaled,
        R8G8B8Sscaled,
        R16G16B16Uscaled,
        R16G16B16Sscaled,
        R32G32B32Uscaled,
        R32G32B32Sscaled,
        R8G8B8A8Uscaled,
        R8G8B8A8Sscaled,
        R16G16B16A16Uscaled,
        R16G16B16A16Sscaled,
        R32G32B32A32Uscaled,
        R32G32B32A32Sscaled,
        R10G10B10A2Snorm,
        R10G10B10A2Sint,
        R10G10B10A2Uscaled,
        R10G10B10A2Sscaled,
        R8G8B8X8Unorm,
        R8G8B8X8Snorm,
        R8G8B8X8Uint,
        R8G8B8X8Sint,
        R16G16B16X16Float,
        R16G16B16X16Unorm,
        R16G16B16X16Snorm,
        R16G16B16X16Uint,
        R16G16B16X16Sint,
        R32G32B32X32Float,
        R32G32B32X32Uint,
        R32G32B32X32Sint,
        Astc4x4Unorm,
        Astc5x4Unorm,
        Astc5x5Unorm,
        Astc6x5Unorm,
        Astc6x6Unorm,
        Astc8x5Unorm,
        Astc8x6Unorm,
        Astc8x8Unorm,
        Astc10x5Unorm,
        Astc10x6Unorm,
        Astc10x8Unorm,
        Astc10x10Unorm,
        Astc12x10Unorm,
        Astc12x12Unorm,
        Astc4x4Srgb,
        Astc5x4Srgb,
        Astc5x5Srgb,
        Astc6x5Srgb,
        Astc6x6Srgb,
        Astc8x5Srgb,
        Astc8x6Srgb,
        Astc8x8Srgb,
        Astc10x5Srgb,
        Astc10x6Srgb,
        Astc10x8Srgb,
        Astc10x10Srgb,
        Astc12x10Srgb,
        Astc12x12Srgb,
        B5G6R5Unorm,
        B5G5R5X1Unorm,
        B5G5R5A1Unorm,
        A1B5G5R5Unorm,
        B8G8R8X8Unorm,
        B8G8R8A8Unorm,
        B8G8R8X8Srgb,
        B8G8R8A8Srgb
    }

    public static class FormatExtensions
    {
        public static bool IsAstc(this Format format)
        {
            return format.IsAstcUnorm() || format.IsAstcSrgb();
        }

        public static bool IsAstcUnorm(this Format format)
        {
            switch (format)
            {
                case Format.Astc4x4Unorm:
                case Format.Astc5x4Unorm:
                case Format.Astc5x5Unorm:
                case Format.Astc6x5Unorm:
                case Format.Astc6x6Unorm:
                case Format.Astc8x5Unorm:
                case Format.Astc8x6Unorm:
                case Format.Astc8x8Unorm:
                case Format.Astc10x5Unorm:
                case Format.Astc10x6Unorm:
                case Format.Astc10x8Unorm:
                case Format.Astc10x10Unorm:
                case Format.Astc12x10Unorm:
                case Format.Astc12x12Unorm:
                    return true;
            }

            return false;
        }

        public static bool IsAstcSrgb(this Format format)
        {
            switch (format)
            {
                case Format.Astc4x4Srgb:
                case Format.Astc5x4Srgb:
                case Format.Astc5x5Srgb:
                case Format.Astc6x5Srgb:
                case Format.Astc6x6Srgb:
                case Format.Astc8x5Srgb:
                case Format.Astc8x6Srgb:
                case Format.Astc8x8Srgb:
                case Format.Astc10x5Srgb:
                case Format.Astc10x6Srgb:
                case Format.Astc10x8Srgb:
                case Format.Astc10x10Srgb:
                case Format.Astc12x10Srgb:
                case Format.Astc12x12Srgb:
                    return true;
            }

            return false;
        }
    }
}
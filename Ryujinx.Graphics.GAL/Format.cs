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
        Bc6HSfloat,
        Bc6HUfloat,
        Etc2RgbUnorm,
        Etc2RgbaUnorm,
        Etc2RgbSrgb,
        Etc2RgbaSrgb,
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
        /// <summary>
        /// Checks if the texture format is an ASTC format.
        /// </summary>
        /// <param name="format">Texture format</param>
        /// <returns>True if the texture format is an ASTC format, false otherwise</returns>
        public static bool IsAstc(this Format format)
        {
            return format.IsAstcUnorm() || format.IsAstcSrgb();
        }

        /// <summary>
        /// Checks if the texture format is an ASTC Unorm format.
        /// </summary>
        /// <param name="format">Texture format</param>
        /// <returns>True if the texture format is an ASTC Unorm format, false otherwise</returns>
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

        /// <summary>
        /// Checks if the texture format is an ASTC SRGB format.
        /// </summary>
        /// <param name="format">Texture format</param>
        /// <returns>True if the texture format is an ASTC SRGB format, false otherwise</returns>
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

        /// <summary>
        /// Checks if the texture format is a BGRA format with 8-bit components.
        /// </summary>
        /// <param name="format">Texture format</param>
        /// <returns>True if the texture format is a BGRA format with 8-bit components, false otherwise</returns>
        public static bool IsBgra8(this Format format)
        {
            switch (format)
            {
                case Format.B8G8R8X8Unorm:
                case Format.B8G8R8A8Unorm:
                case Format.B8G8R8X8Srgb:
                case Format.B8G8R8A8Srgb:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the texture format is a depth, stencil or depth-stencil format.
        /// </summary>
        /// <param name="format">Texture format</param>
        /// <returns>True if the format is a depth, stencil or depth-stencil format, false otherwise</returns>
        public static bool IsDepthOrStencil(this Format format)
        {
            switch (format)
            {
                case Format.D16Unorm:
                case Format.D24UnormS8Uint:
                case Format.D24X8Unorm:
                case Format.D32Float:
                case Format.D32FloatS8Uint:
                case Format.S8Uint:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the texture format is an unsigned integer color format.
        /// </summary>
        /// <param name="format">Texture format</param>
        /// <returns>True if the texture format is an unsigned integer color format, false otherwise</returns>
        public static bool IsUint(this Format format)
        {
            switch (format)
            {
                case Format.R8Uint:
                case Format.R16Uint:
                case Format.R32Uint:
                case Format.R8G8Uint:
                case Format.R16G16Uint:
                case Format.R32G32Uint:
                case Format.R8G8B8Uint:
                case Format.R16G16B16Uint:
                case Format.R32G32B32Uint:
                case Format.R8G8B8A8Uint:
                case Format.R16G16B16A16Uint:
                case Format.R32G32B32A32Uint:
                case Format.R10G10B10A2Uint:
                case Format.R8G8B8X8Uint:
                case Format.R16G16B16X16Uint:
                case Format.R32G32B32X32Uint:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the texture format is a signed integer color format.
        /// </summary>
        /// <param name="format">Texture format</param>
        /// <returns>True if the texture format is a signed integer color format, false otherwise</returns>
        public static bool IsSint(this Format format)
        {
            switch (format)
            {
                case Format.R8Sint:
                case Format.R16Sint:
                case Format.R32Sint:
                case Format.R8G8Sint:
                case Format.R16G16Sint:
                case Format.R32G32Sint:
                case Format.R8G8B8Sint:
                case Format.R16G16B16Sint:
                case Format.R32G32B32Sint:
                case Format.R8G8B8A8Sint:
                case Format.R16G16B16A16Sint:
                case Format.R32G32B32A32Sint:
                case Format.R10G10B10A2Sint:
                case Format.R8G8B8X8Sint:
                case Format.R16G16B16X16Sint:
                case Format.R32G32B32X32Sint:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the texture format is an integer color format.
        /// </summary>
        /// <param name="format">Texture format</param>
        /// <returns>True if the texture format is an integer color format, false otherwise</returns>
        public static bool IsInteger(this Format format)
        {
            return format.IsUint() || format.IsSint();
        }

        /// <summary>
        /// Checks if the texture format is a BC4 compressed format.
        /// </summary>
        /// <param name="format">Texture format</param>
        /// <returns>True if the texture format is a BC4 compressed format, false otherwise</returns>
        public static bool IsBc4(this Format format)
        {
            return format == Format.Bc4Unorm || format == Format.Bc4Snorm;
        }

        /// <summary>
        /// Checks if the texture format is a BC5 compressed format.
        /// </summary>
        /// <param name="format">Texture format</param>
        /// <returns>True if the texture format is a BC5 compressed format, false otherwise</returns>
        public static bool IsBc5(this Format format)
        {
            return format == Format.Bc5Unorm || format == Format.Bc5Snorm;
        }
    }
}
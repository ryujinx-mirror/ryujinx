using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Texture;
using System;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Texture format compatibility checks.
    /// </summary>
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

        /// <summary>
        /// Finds the appropriate depth format for a copy texture if the source texture has a depth format.
        /// </summary>
        /// <param name="dstTextureFormat">Destination CopyTexture Format</param>
        /// <param name="srcTextureFormat">Source Texture Format</param>
        /// <returns>Derived RtFormat if srcTextureFormat is a depth format, otherwise return dstTextureFormat.</returns>
        public static RtFormat DeriveDepthFormat(RtFormat dstTextureFormat, Format srcTextureFormat)
        {
            return srcTextureFormat switch
            {
                Format.S8Uint => RtFormat.S8Uint,
                Format.D16Unorm => RtFormat.D16Unorm,
                Format.D24X8Unorm => RtFormat.D24Unorm,
                Format.D32Float => RtFormat.D32Float,
                Format.D24UnormS8Uint => RtFormat.D24UnormS8Uint,
                Format.D32FloatS8Uint => RtFormat.D32FloatS8Uint,
                _ => dstTextureFormat
            };
        }

        /// <summary>
        /// Checks if two formats are compatible, according to the host API copy format compatibility rules.
        /// </summary>
        /// <param name="lhs">First comparand</param>
        /// <param name="rhs">Second comparand</param>
        /// <returns>True if the formats are compatible, false otherwise</returns>
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

        /// <summary>
        /// Checks if the texture format matches with the specified texture information.
        /// </summary>
        /// <param name="lhs">Texture information to compare</param>
        /// <param name="rhs">Texture information to compare with</param>
        /// <param name="forSampler">Indicates that the texture will be used for shader sampling</param>
        /// <param name="forCopy">Indicates that the texture will be used as copy source or target</param>
        /// <returns>True if the format matches, with the given comparison rules</returns>
        public static bool FormatMatches(TextureInfo lhs, TextureInfo rhs, bool forSampler, bool forCopy)
        {
            // D32F and R32F texture have the same representation internally,
            // however the R32F format is used to sample from depth textures.
            if (lhs.FormatInfo.Format == Format.D32Float && rhs.FormatInfo.Format == Format.R32Float && (forSampler || forCopy))
            {
                return true;
            }

            if (forCopy)
            {
                // The 2D engine does not support depth-stencil formats, so it will instead
                // use equivalent color formats. We must also consider them as compatible.
                if (lhs.FormatInfo.Format == Format.S8Uint && rhs.FormatInfo.Format == Format.R8Unorm)
                {
                    return true;
                }

                if (lhs.FormatInfo.Format == Format.D16Unorm && rhs.FormatInfo.Format == Format.R16Unorm)
                {
                    return true;
                }

                if ((lhs.FormatInfo.Format == Format.D24UnormS8Uint ||
                     lhs.FormatInfo.Format == Format.D24X8Unorm) && rhs.FormatInfo.Format == Format.B8G8R8A8Unorm)
                {
                    return true;
                }
            }

            return lhs.FormatInfo.Format == rhs.FormatInfo.Format;
        }

        /// <summary>
        /// Checks if the texture layout specified matches with this texture layout.
        /// The layout information is composed of the Stride for linear textures, or GOB block size
        /// for block linear textures.
        /// </summary>
        /// <param name="lhs">Texture information to compare</param>
        /// <param name="rhs">Texture information to compare with</param>
        /// <returns>True if the layout matches, false otherwise</returns>
        public static bool LayoutMatches(TextureInfo lhs, TextureInfo rhs)
        {
            if (lhs.IsLinear != rhs.IsLinear)
            {
                return false;
            }

            // For linear textures, gob block sizes are ignored.
            // For block linear textures, the stride is ignored.
            if (rhs.IsLinear)
            {
                return lhs.Stride == rhs.Stride;
            }
            else
            {
                return lhs.GobBlocksInY == rhs.GobBlocksInY &&
                       lhs.GobBlocksInZ == rhs.GobBlocksInZ;
            }
        }

        /// <summary>
        /// Checks if the view sizes of a two given texture informations match.
        /// </summary>
        /// <param name="lhs">Texture information of the texture view</param>
        /// <param name="rhs">Texture information of the texture view to match against</param>
        /// <param name="level">Mipmap level of the texture view in relation to this texture</param>
        /// <param name="isCopy">True to check for copy compatibility rather than view compatibility</param>
        /// <returns>True if the sizes are compatible, false otherwise</returns>
        public static bool ViewSizeMatches(TextureInfo lhs, TextureInfo rhs, int level, bool isCopy)
        {
            Size size = GetAlignedSize(lhs, level);

            Size otherSize = GetAlignedSize(rhs);

            // For copies, we can copy a subset of the 3D texture slices,
            // so the depth may be different in this case.
            if (!isCopy && rhs.Target == Target.Texture3D && size.Depth != otherSize.Depth)
            {
                return false;
            }

            return size.Width  == otherSize.Width &&
                   size.Height == otherSize.Height;
        }

        /// <summary>
        /// Checks if the texture sizes of the supplied texture informations match.
        /// </summary>
        /// <param name="lhs">Texture information to compare</param>
        /// <param name="rhs">Texture information to compare with</param>
        /// <returns>True if the size matches, false otherwise</returns>
        public static bool SizeMatches(TextureInfo lhs, TextureInfo rhs)
        {
            return SizeMatches(lhs, rhs, alignSizes: false);
        }

        /// <summary>
        /// Checks if the texture sizes of the supplied texture informations match the given level
        /// </summary>
        /// <param name="lhs">Texture information to compare</param>
        /// <param name="rhs">Texture information to compare with</param>
        /// <param name="level">Mipmap level of this texture to compare with</param>
        /// <returns>True if the size matches with the level, false otherwise</returns>
        public static bool SizeMatches(TextureInfo lhs, TextureInfo rhs, int level)
        {
            return Math.Max(1, lhs.Width >> level)      == rhs.Width &&
                   Math.Max(1, lhs.Height >> level)     == rhs.Height &&
                   Math.Max(1, lhs.GetDepth() >> level) == rhs.GetDepth();
        }

        /// <summary>
        /// Checks if the texture sizes of the supplied texture informations match.
        /// </summary>
        /// <param name="lhs">Texture information to compare</param>
        /// <param name="rhs">Texture information to compare with</param>
        /// <param name="alignSizes">True to align the sizes according to the texture layout for comparison</param>
        /// <returns>True if the sizes matches, false otherwise</returns>
        public static bool SizeMatches(TextureInfo lhs, TextureInfo rhs, bool alignSizes)
        {
            if (lhs.GetLayers() != rhs.GetLayers())
            {
                return false;
            }

            if (alignSizes)
            {
                Size size0 = GetAlignedSize(lhs);
                Size size1 = GetAlignedSize(rhs);

                return size0.Width  == size1.Width &&
                       size0.Height == size1.Height &&
                       size0.Depth  == size1.Depth;
            }
            else
            {
                return lhs.Width      == rhs.Width &&
                       lhs.Height     == rhs.Height &&
                       lhs.GetDepth() == rhs.GetDepth();
            }
        }

        /// <summary>
        /// Gets the aligned sizes of the specified texture information.
        /// The alignment depends on the texture layout and format bytes per pixel.
        /// </summary>
        /// <param name="info">Texture information to calculate the aligned size from</param>
        /// <param name="level">Mipmap level for texture views</param>
        /// <returns>The aligned texture size</returns>
        public static Size GetAlignedSize(TextureInfo info, int level = 0)
        {
            int width = Math.Max(1, info.Width >> level);
            int height = Math.Max(1, info.Height >> level);

            if (info.IsLinear)
            {
                return SizeCalculator.GetLinearAlignedSize(
                    width,
                    height,
                    info.FormatInfo.BlockWidth,
                    info.FormatInfo.BlockHeight,
                    info.FormatInfo.BytesPerPixel);
            }
            else
            {
                int depth = Math.Max(1, info.GetDepth() >> level);

                return SizeCalculator.GetBlockLinearAlignedSize(
                    width,
                    height,
                    depth,
                    info.FormatInfo.BlockWidth,
                    info.FormatInfo.BlockHeight,
                    info.FormatInfo.BytesPerPixel,
                    info.GobBlocksInY,
                    info.GobBlocksInZ,
                    info.GobBlocksInTileX);
            }
        }

        /// <summary>
        /// Check if it's possible to create a view with the layout of the second texture information from the first.
        /// The layout information is composed of the Stride for linear textures, or GOB block size
        /// for block linear textures.
        /// </summary>
        /// <param name="lhs">Texture information of the texture view</param>
        /// <param name="rhs">Texture information of the texture view to compare against</param>
        /// <param name="level">Start level of the texture view, in relation with the first texture</param>
        /// <returns>True if the layout is compatible, false otherwise</returns>
        public static bool ViewLayoutCompatible(TextureInfo lhs, TextureInfo rhs, int level)
        {
            if (lhs.IsLinear != rhs.IsLinear)
            {
                return false;
            }

            // For linear textures, gob block sizes are ignored.
            // For block linear textures, the stride is ignored.
            if (rhs.IsLinear)
            {
                int width = Math.Max(1, lhs.Width >> level);
                int stride = width * lhs.FormatInfo.BytesPerPixel;
                stride = BitUtils.AlignUp(stride, 32);

                return stride == rhs.Stride;
            }
            else
            {
                int height = Math.Max(1, lhs.Height >> level);
                int depth = Math.Max(1, lhs.GetDepth() >> level);

                (int gobBlocksInY, int gobBlocksInZ) = SizeCalculator.GetMipGobBlockSizes(
                    height,
                    depth,
                    lhs.FormatInfo.BlockHeight,
                    lhs.GobBlocksInY,
                    lhs.GobBlocksInZ);

                return gobBlocksInY == rhs.GobBlocksInY &&
                       gobBlocksInZ == rhs.GobBlocksInZ;
            }
        }

        /// <summary>
        /// Checks if the view format of the first texture format is compatible with the format of the second.
        /// In general, the formats are considered compatible if the bytes per pixel values are equal,
        /// but there are more complex rules for some formats, like compressed or depth-stencil formats.
        /// This follows the host API copy compatibility rules.
        /// </summary>
        /// <param name="lhs">Texture information of the texture view</param>
        /// <param name="rhs">Texture information of the texture view</param>
        /// <returns>True if the formats are compatible, false otherwise</returns>
        public static bool ViewFormatCompatible(TextureInfo lhs, TextureInfo rhs)
        {
            return FormatCompatible(lhs.FormatInfo, rhs.FormatInfo);
        }

        /// <summary>
        /// Check if the target of the first texture view information is compatible with the target of the second texture view information.
        /// This follows the host API target compatibility rules.
        /// </summary>
        /// <param name="lhs">Texture information of the texture view</param
        /// <param name="rhs">Texture information of the texture view</param>
        /// <param name="isCopy">True to check for copy rather than view compatibility</param>
        /// <returns>True if the targets are compatible, false otherwise</returns>
        public static bool ViewTargetCompatible(TextureInfo lhs, TextureInfo rhs, bool isCopy)
        {
            switch (lhs.Target)
            {
                case Target.Texture1D:
                case Target.Texture1DArray:
                    return rhs.Target == Target.Texture1D ||
                           rhs.Target == Target.Texture1DArray;

                case Target.Texture2D:
                    return rhs.Target == Target.Texture2D ||
                           rhs.Target == Target.Texture2DArray;

                case Target.Texture2DArray:
                case Target.Cubemap:
                case Target.CubemapArray:
                    return rhs.Target == Target.Texture2D ||
                           rhs.Target == Target.Texture2DArray ||
                           rhs.Target == Target.Cubemap ||
                           rhs.Target == Target.CubemapArray;

                case Target.Texture2DMultisample:
                case Target.Texture2DMultisampleArray:
                    return rhs.Target == Target.Texture2DMultisample ||
                           rhs.Target == Target.Texture2DMultisampleArray;

                case Target.Texture3D:
                    return rhs.Target == Target.Texture3D ||
                          (rhs.Target == Target.Texture2D && isCopy);
            }

            return false;
        }

        /// <summary>
        /// Checks if the texture shader sampling parameters of two texture informations match.
        /// </summary>
        /// <param name="lhs">Texture information to compare</param>
        /// <param name="rhs">Texture information to compare with</param>
        /// <returns>True if the texture shader sampling parameters matches, false otherwise</returns>
        public static bool SamplerParamsMatches(TextureInfo lhs, TextureInfo rhs)
        {
            return lhs.DepthStencilMode == rhs.DepthStencilMode &&
                   lhs.SwizzleR         == rhs.SwizzleR &&
                   lhs.SwizzleG         == rhs.SwizzleG &&
                   lhs.SwizzleB         == rhs.SwizzleB &&
                   lhs.SwizzleA         == rhs.SwizzleA;
        }

        /// <summary>
        /// Check if the texture target and samples count (for multisampled textures) matches.
        /// </summary>
        /// <param name="first">Texture information to compare with</param>
        /// <param name="rhs">Texture information to compare with</param>
        /// <returns>True if the texture target and samples count matches, false otherwise</returns>
        public static bool TargetAndSamplesCompatible(TextureInfo lhs, TextureInfo rhs)
        {
            return lhs.Target     == rhs.Target &&
                   lhs.SamplesInX == rhs.SamplesInX &&
                   lhs.SamplesInY == rhs.SamplesInY;
        }

        /// <summary>
        /// Gets the texture format class, for compressed textures, or Unclassified otherwise.
        /// </summary>
        /// <param name="format">The format</param>
        /// <returns>Format class</returns>
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

        /// <summary>
        /// Checks if the format is a depth-stencil texture format.
        /// </summary>
        /// <param name="format">Format to check</param>
        /// <returns>True if the format is a depth-stencil format (including depth only), false otherwise</returns>
        private static bool IsDsFormat(Format format)
        {
            switch (format)
            {
                case Format.D16Unorm:
                case Format.D24X8Unorm:
                case Format.D24UnormS8Uint:
                case Format.D32Float:
                case Format.D32FloatS8Uint:
                case Format.S8Uint:
                    return true;
            }

            return false;
        }
    }
}
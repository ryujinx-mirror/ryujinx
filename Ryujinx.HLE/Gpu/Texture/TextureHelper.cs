using ChocolArm64.Memory;
using Ryujinx.Graphics.Gal;
using Ryujinx.HLE.Gpu.Memory;
using System;

namespace Ryujinx.HLE.Gpu.Texture
{
    static class TextureHelper
    {
        public static ISwizzle GetSwizzle(TextureInfo Texture, int BlockWidth, int Bpp)
        {
            int Width = (Texture.Width + (BlockWidth - 1)) / BlockWidth;

            int AlignMask = Texture.TileWidth * (64 / Bpp) - 1;

            Width = (Width + AlignMask) & ~AlignMask;

            switch (Texture.Swizzle)
            {
                case TextureSwizzle._1dBuffer:
                case TextureSwizzle.Pitch:
                case TextureSwizzle.PitchColorKey:
                     return new LinearSwizzle(Texture.Pitch, Bpp);

                case TextureSwizzle.BlockLinear:
                case TextureSwizzle.BlockLinearColorKey:
                    return new BlockLinearSwizzle(Width, Bpp, Texture.BlockHeight);
            }

            throw new NotImplementedException(Texture.Swizzle.ToString());
        }

        public static int GetTextureSize(GalImage Image)
        {
            switch (Image.Format)
            {
                case GalImageFormat.R32G32B32A32_SFLOAT:
                case GalImageFormat.R32G32B32A32_SINT:
                case GalImageFormat.R32G32B32A32_UINT:
                    return Image.Width * Image.Height * 16;

                case GalImageFormat.R16G16B16A16_SFLOAT:
                case GalImageFormat.R16G16B16A16_SINT:
                case GalImageFormat.R16G16B16A16_SNORM:
                case GalImageFormat.R16G16B16A16_UINT:
                case GalImageFormat.R16G16B16A16_UNORM:
                case GalImageFormat.D32_SFLOAT_S8_UINT:
                case GalImageFormat.R32G32_SFLOAT:
                case GalImageFormat.R32G32_SINT:
                case GalImageFormat.R32G32_UINT:
                    return Image.Width * Image.Height * 8;

                case GalImageFormat.A8B8G8R8_SINT_PACK32:
                case GalImageFormat.A8B8G8R8_SNORM_PACK32:
                case GalImageFormat.A8B8G8R8_UINT_PACK32:
                case GalImageFormat.A8B8G8R8_UNORM_PACK32:
                case GalImageFormat.A8B8G8R8_SRGB_PACK32:
                case GalImageFormat.A2B10G10R10_SINT_PACK32:
                case GalImageFormat.A2B10G10R10_SNORM_PACK32:
                case GalImageFormat.A2B10G10R10_UINT_PACK32:
                case GalImageFormat.A2B10G10R10_UNORM_PACK32:
                case GalImageFormat.R16G16_SFLOAT:
                case GalImageFormat.R16G16_SINT:
                case GalImageFormat.R16G16_SNORM:
                case GalImageFormat.R16G16_UINT:
                case GalImageFormat.R16G16_UNORM:
                case GalImageFormat.R32_SFLOAT:
                case GalImageFormat.R32_SINT:
                case GalImageFormat.R32_UINT:
                case GalImageFormat.D32_SFLOAT:
                case GalImageFormat.B10G11R11_UFLOAT_PACK32:
                case GalImageFormat.D24_UNORM_S8_UINT:
                    return Image.Width * Image.Height * 4;

                case GalImageFormat.B4G4R4A4_UNORM_PACK16:
                case GalImageFormat.A1R5G5B5_UNORM_PACK16:
                case GalImageFormat.B5G6R5_UNORM_PACK16:
                case GalImageFormat.R8G8_SINT:
                case GalImageFormat.R8G8_SNORM:
                case GalImageFormat.R8G8_UINT:
                case GalImageFormat.R8G8_UNORM:
                case GalImageFormat.R16_SFLOAT:
                case GalImageFormat.R16_SINT:
                case GalImageFormat.R16_SNORM:
                case GalImageFormat.R16_UINT:
                case GalImageFormat.R16_UNORM:
                case GalImageFormat.D16_UNORM:
                    return Image.Width * Image.Height * 2;

                case GalImageFormat.R8_SINT:
                case GalImageFormat.R8_SNORM:
                case GalImageFormat.R8_UINT:
                case GalImageFormat.R8_UNORM:
                    return Image.Width * Image.Height;

                case GalImageFormat.BC1_RGBA_UNORM_BLOCK:
                case GalImageFormat.BC4_SNORM_BLOCK:
                case GalImageFormat.BC4_UNORM_BLOCK:
                {
                    return CompressedTextureSize(Image.Width, Image.Height, 4, 4, 8);
                }

                case GalImageFormat.BC6H_SFLOAT_BLOCK:
                case GalImageFormat.BC6H_UFLOAT_BLOCK:
                case GalImageFormat.BC7_UNORM_BLOCK:
                case GalImageFormat.BC2_UNORM_BLOCK:
                case GalImageFormat.BC3_UNORM_BLOCK:
                case GalImageFormat.BC5_SNORM_BLOCK:
                case GalImageFormat.BC5_UNORM_BLOCK:
                case GalImageFormat.ASTC_4x4_UNORM_BLOCK:
                {
                    return CompressedTextureSize(Image.Width, Image.Height, 4, 4, 16);
                }

                case GalImageFormat.ASTC_5x5_UNORM_BLOCK:
                {
                    return CompressedTextureSize(Image.Width, Image.Height, 5, 5, 16);
                }

                case GalImageFormat.ASTC_6x6_UNORM_BLOCK:
                {
                    return CompressedTextureSize(Image.Width, Image.Height, 6, 6, 16);
                }

                case GalImageFormat.ASTC_8x8_UNORM_BLOCK:
                {
                    return CompressedTextureSize(Image.Width, Image.Height, 8, 8, 16);
                }

                case GalImageFormat.ASTC_10x10_UNORM_BLOCK:
                {
                    return CompressedTextureSize(Image.Width, Image.Height, 10, 10, 16);
                }

                case GalImageFormat.ASTC_12x12_UNORM_BLOCK:
                {
                    return CompressedTextureSize(Image.Width, Image.Height, 12, 12, 16);
                }

                case GalImageFormat.ASTC_5x4_UNORM_BLOCK:
                {
                    return CompressedTextureSize(Image.Width, Image.Height, 5, 4, 16);
                }

                case GalImageFormat.ASTC_6x5_UNORM_BLOCK:
                {
                    return CompressedTextureSize(Image.Width, Image.Height, 6, 5, 16);
                }

                case GalImageFormat.ASTC_8x6_UNORM_BLOCK:
                {
                    return CompressedTextureSize(Image.Width, Image.Height, 8, 6, 16);
                }

                case GalImageFormat.ASTC_10x8_UNORM_BLOCK:
                {
                    return CompressedTextureSize(Image.Width, Image.Height, 10, 8, 16);
                }

                case GalImageFormat.ASTC_12x10_UNORM_BLOCK:
                {
                    return CompressedTextureSize(Image.Width, Image.Height, 12, 10, 16);
                }

                case GalImageFormat.ASTC_8x5_UNORM_BLOCK:
                {
                    return CompressedTextureSize(Image.Width, Image.Height, 8, 5, 16);
                }

                case GalImageFormat.ASTC_10x5_UNORM_BLOCK:
                {
                    return CompressedTextureSize(Image.Width, Image.Height, 10, 5, 16);
                }

                case GalImageFormat.ASTC_10x6_UNORM_BLOCK:
                {
                    return CompressedTextureSize(Image.Width, Image.Height, 10, 6, 16);
                }
            }

            throw new NotImplementedException("0x" + Image.Format.ToString("x2"));
        }

        public static int CompressedTextureSize(int TextureWidth, int TextureHeight, int BlockWidth, int BlockHeight, int Bpb)
        {
            int W = (TextureWidth  + (BlockWidth - 1)) / BlockWidth;
            int H = (TextureHeight + (BlockHeight - 1)) / BlockHeight;

            return W * H * Bpb;
        }

        public static (AMemory Memory, long Position) GetMemoryAndPosition(
            IAMemory Memory,
            long     Position)
        {
            if (Memory is NvGpuVmm Vmm)
            {
                return (Vmm.Memory, Vmm.GetPhysicalAddress(Position));
            }

            return ((AMemory)Memory, Position);
        }
    }
}

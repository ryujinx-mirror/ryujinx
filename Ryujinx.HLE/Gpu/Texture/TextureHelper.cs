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

        public static int GetTextureSize(GalTexture Texture)
        {
            switch (Texture.Format)
            {
                case GalTextureFormat.R32G32B32A32:
                    return Texture.Width * Texture.Height * 16;

                case GalTextureFormat.R16G16B16A16:
                    return Texture.Width * Texture.Height * 8;

                case GalTextureFormat.A8B8G8R8:
                case GalTextureFormat.A2B10G10R10:
                case GalTextureFormat.R32:
                case GalTextureFormat.ZF32:
                case GalTextureFormat.BF10GF11RF11:
                case GalTextureFormat.Z24S8:
                    return Texture.Width * Texture.Height * 4;

                case GalTextureFormat.A1B5G5R5:
                case GalTextureFormat.B5G6R5:
                case GalTextureFormat.G8R8:
                case GalTextureFormat.R16:
                    return Texture.Width * Texture.Height * 2;

                case GalTextureFormat.R8:
                    return Texture.Width * Texture.Height;

                case GalTextureFormat.BC1:
                case GalTextureFormat.BC4:
                {
                    return CompressedTextureSize(Texture.Width, Texture.Height, 4, 4, 8);
                }

                case GalTextureFormat.BC6H_SF16:
                case GalTextureFormat.BC6H_UF16:
                case GalTextureFormat.BC7U:
                case GalTextureFormat.BC2:
                case GalTextureFormat.BC3:
                case GalTextureFormat.BC5:
                case GalTextureFormat.Astc2D4x4:
                {
                    return CompressedTextureSize(Texture.Width, Texture.Height, 4, 4, 16);
                }

                case GalTextureFormat.Astc2D5x5:
                {
                    return CompressedTextureSize(Texture.Width, Texture.Height, 5, 5, 16);
                }

                case GalTextureFormat.Astc2D6x6:
                {
                    return CompressedTextureSize(Texture.Width, Texture.Height, 6, 6, 16);
                }

                case GalTextureFormat.Astc2D8x8:
                {
                    return CompressedTextureSize(Texture.Width, Texture.Height, 8, 8, 16);
                }

                case GalTextureFormat.Astc2D10x10:
                {
                    return CompressedTextureSize(Texture.Width, Texture.Height, 10, 10, 16);
                }

                case GalTextureFormat.Astc2D12x12:
                {
                    return CompressedTextureSize(Texture.Width, Texture.Height, 12, 12, 16);
                }

                case GalTextureFormat.Astc2D5x4:
                {
                    return CompressedTextureSize(Texture.Width, Texture.Height, 5, 4, 16);
                }

                case GalTextureFormat.Astc2D6x5:
                {
                    return CompressedTextureSize(Texture.Width, Texture.Height, 6, 5, 16);
                }

                case GalTextureFormat.Astc2D8x6:
                {
                    return CompressedTextureSize(Texture.Width, Texture.Height, 8, 6, 16);
                }

                case GalTextureFormat.Astc2D10x8:
                {
                    return CompressedTextureSize(Texture.Width, Texture.Height, 10, 8, 16);
                }

                case GalTextureFormat.Astc2D12x10:
                {
                    return CompressedTextureSize(Texture.Width, Texture.Height, 12, 10, 16);
                }

                case GalTextureFormat.Astc2D8x5:
                {
                    return CompressedTextureSize(Texture.Width, Texture.Height, 8, 5, 16);
                }

                case GalTextureFormat.Astc2D10x5:
                {
                    return CompressedTextureSize(Texture.Width, Texture.Height, 10, 5, 16);
                }

                case GalTextureFormat.Astc2D10x6:
                {
                    return CompressedTextureSize(Texture.Width, Texture.Height, 10, 6, 16);
                }
            }

            throw new NotImplementedException(Texture.Format.ToString());
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

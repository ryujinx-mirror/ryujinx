using ChocolArm64.Memory;
using Ryujinx.Graphics.Gal;
using Ryujinx.HLE.Gpu.Memory;
using System;

namespace Ryujinx.HLE.Gpu.Texture
{
    static class TextureHelper
    {
        public static ISwizzle GetSwizzle(TextureInfo Texture, int Width, int Bpp)
        {
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
                case GalTextureFormat.R32:
                case GalTextureFormat.ZF32:
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
                    int W = (Texture.Width  + 3) / 4;
                    int H = (Texture.Height + 3) / 4;

                    return W * H * 8;
                }

                case GalTextureFormat.BC7U:
                case GalTextureFormat.BC2:
                case GalTextureFormat.BC3:
                case GalTextureFormat.BC5:
                case GalTextureFormat.Astc2D4x4:
                {
                    int W = (Texture.Width  + 3) / 4;
                    int H = (Texture.Height + 3) / 4;

                    return W * H * 16;
                }
            }

            throw new NotImplementedException(Texture.Format.ToString());
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

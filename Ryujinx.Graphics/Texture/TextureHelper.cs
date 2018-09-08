using ChocolArm64.Memory;
using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Memory;
using System;

namespace Ryujinx.Graphics.Texture
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

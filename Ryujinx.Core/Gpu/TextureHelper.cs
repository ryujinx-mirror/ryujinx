using ChocolArm64.Memory;
using System;

namespace Ryujinx.Core.Gpu
{
    static class TextureHelper
    {
        public static ISwizzle GetSwizzle(Texture Texture, int Width, int Bpp)
        {
            switch (Texture.Swizzle)
            {
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

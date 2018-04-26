using System;

namespace Ryujinx.Graphics.Gpu
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
    }
}

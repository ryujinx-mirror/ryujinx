using System;

namespace Ryujinx.Graphics.Shader
{
    [Flags]
    public enum TextureTarget
    {
        Texture1D,
        Texture2D,
        Texture3D,
        TextureCube,

        Mask = 0xff,

        Array       = 1 << 8,
        Multisample = 1 << 9,
        Shadow      = 1 << 10
    }

    static class TextureTargetExtensions
    {
        public static int GetDimensions(this TextureTarget type)
        {
            switch (type & TextureTarget.Mask)
            {
                case TextureTarget.Texture1D:   return 1;
                case TextureTarget.Texture2D:   return 2;
                case TextureTarget.Texture3D:   return 3;
                case TextureTarget.TextureCube: return 3;
            }

            throw new ArgumentException($"Invalid texture type \"{type}\".");
        }
    }
}
using System;

namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    [Flags]
    enum TextureType
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

    static class TextureTypeExtensions
    {
        public static int GetCoordsCount(this TextureType type)
        {
            switch (type & TextureType.Mask)
            {
                case TextureType.Texture1D:   return 1;
                case TextureType.Texture2D:   return 2;
                case TextureType.Texture3D:   return 3;
                case TextureType.TextureCube: return 3;
            }

            throw new ArgumentException($"Invalid texture type \"{type}\".");
        }
    }
}
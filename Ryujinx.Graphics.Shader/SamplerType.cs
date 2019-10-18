using System;

namespace Ryujinx.Graphics.Shader
{
    [Flags]
    public enum SamplerType
    {
        Texture1D,
        TextureBuffer,
        Texture2D,
        Texture3D,
        TextureCube,

        Mask = 0xff,

        Array       = 1 << 8,
        Multisample = 1 << 9,
        Shadow      = 1 << 10
    }

    static class SamplerTypeExtensions
    {
        public static int GetDimensions(this SamplerType type)
        {
            switch (type & SamplerType.Mask)
            {
                case SamplerType.Texture1D:     return 1;
                case SamplerType.TextureBuffer: return 1;
                case SamplerType.Texture2D:     return 2;
                case SamplerType.Texture3D:     return 3;
                case SamplerType.TextureCube:   return 3;
            }

            throw new ArgumentException($"Invalid texture type \"{type}\".");
        }
    }
}
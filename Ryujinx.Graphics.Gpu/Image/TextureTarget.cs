using Ryujinx.Graphics.GAL.Texture;

namespace Ryujinx.Graphics.Gpu.Image
{
    enum TextureTarget
    {
        Texture1D,
        Texture2D,
        Texture3D,
        Cubemap,
        Texture1DArray,
        Texture2DArray,
        TextureBuffer,
        Texture2DLinear,
        CubemapArray
    }

    static class TextureTargetConverter
    {
        public static Target Convert(this TextureTarget target, bool isMultisample)
        {
            if (isMultisample)
            {
                switch (target)
                {
                    case TextureTarget.Texture2D:      return Target.Texture2DMultisample;
                    case TextureTarget.Texture2DArray: return Target.Texture2DMultisampleArray;
                }
            }
            else
            {
                switch (target)
                {
                    case TextureTarget.Texture1D:       return Target.Texture1D;
                    case TextureTarget.Texture2D:       return Target.Texture2D;
                    case TextureTarget.Texture2DLinear: return Target.Texture2D;
                    case TextureTarget.Texture3D:       return Target.Texture3D;
                    case TextureTarget.Texture1DArray:  return Target.Texture1DArray;
                    case TextureTarget.Texture2DArray:  return Target.Texture2DArray;
                    case TextureTarget.Cubemap:         return Target.Cubemap;
                    case TextureTarget.CubemapArray:    return Target.CubemapArray;
                    case TextureTarget.TextureBuffer:   return Target.TextureBuffer;
                }
            }

            return Target.Texture1D;
        }
    }
}
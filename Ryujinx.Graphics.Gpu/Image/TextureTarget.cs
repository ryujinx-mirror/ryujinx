using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Texture target.
    /// </summary>
    enum TextureTarget : byte
    {
        Texture1D,
        Texture2D,
        Texture3D,
        Cubemap,
        Texture1DArray,
        Texture2DArray,
        TextureBuffer,
        Texture2DRect,
        CubemapArray
    }

    static class TextureTargetConverter
    {
        /// <summary>
        /// Converts the texture target enum to a host compatible, Graphics Abstraction Layer enum.
        /// </summary>
        /// <param name="target">The target enum to convert</param>
        /// <param name="isMultisample">True if the texture is a multisampled texture</param>
        /// <returns>The host compatible texture target</returns>
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
                    case TextureTarget.Texture2DRect:   return Target.Texture2D;
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
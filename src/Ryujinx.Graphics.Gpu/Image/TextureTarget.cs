using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;

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
        CubemapArray,
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
                    case TextureTarget.Texture2D:
                        return Target.Texture2DMultisample;
                    case TextureTarget.Texture2DArray:
                        return Target.Texture2DMultisampleArray;
                }
            }
            else
            {
                switch (target)
                {
                    case TextureTarget.Texture1D:
                        return Target.Texture1D;
                    case TextureTarget.Texture2D:
                        return Target.Texture2D;
                    case TextureTarget.Texture2DRect:
                        return Target.Texture2D;
                    case TextureTarget.Texture3D:
                        return Target.Texture3D;
                    case TextureTarget.Texture1DArray:
                        return Target.Texture1DArray;
                    case TextureTarget.Texture2DArray:
                        return Target.Texture2DArray;
                    case TextureTarget.Cubemap:
                        return Target.Cubemap;
                    case TextureTarget.CubemapArray:
                        return Target.CubemapArray;
                    case TextureTarget.TextureBuffer:
                        return Target.TextureBuffer;
                }
            }

            return Target.Texture1D;
        }

        /// <summary>
        /// Converts the texture target enum to a shader sampler type.
        /// </summary>
        /// <param name="target">The target enum to convert</param>
        /// <returns>The shader sampler type</returns>
        public static SamplerType ConvertSamplerType(this TextureTarget target)
        {
            return target switch
            {
                TextureTarget.Texture1D => SamplerType.Texture1D,
                TextureTarget.Texture2D => SamplerType.Texture2D,
                TextureTarget.Texture3D => SamplerType.Texture3D,
                TextureTarget.Cubemap => SamplerType.TextureCube,
                TextureTarget.Texture1DArray => SamplerType.Texture1D | SamplerType.Array,
                TextureTarget.Texture2DArray => SamplerType.Texture2D | SamplerType.Array,
                TextureTarget.TextureBuffer => SamplerType.TextureBuffer,
                TextureTarget.Texture2DRect => SamplerType.Texture2D,
                TextureTarget.CubemapArray => SamplerType.TextureCube | SamplerType.Array,
                _ => SamplerType.Texture2D,
            };
        }
    }
}

using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.Gpu.Engine
{
    /// <summary>
    /// Shader texture properties conversion methods.
    /// </summary>
    static class ShaderTexture
    {
        /// <summary>
        /// Gets a texture target from a sampler type.
        /// </summary>
        /// <param name="type">Sampler type</param>
        /// <returns>Texture target value</returns>
        public static Target GetTarget(SamplerType type)
        {
            type &= ~SamplerType.Shadow;

            switch (type)
            {
                case SamplerType.Texture1D:
                    return Target.Texture1D;

                case SamplerType.TextureBuffer:
                    return Target.TextureBuffer;

                case SamplerType.Texture1D | SamplerType.Array:
                    return Target.Texture1DArray;

                case SamplerType.Texture2D:
                    return Target.Texture2D;

                case SamplerType.Texture2D | SamplerType.Array:
                    return Target.Texture2DArray;

                case SamplerType.Texture2D | SamplerType.Multisample:
                    return Target.Texture2DMultisample;

                case SamplerType.Texture2D | SamplerType.Multisample | SamplerType.Array:
                    return Target.Texture2DMultisampleArray;

                case SamplerType.Texture3D:
                    return Target.Texture3D;

                case SamplerType.TextureCube:
                    return Target.Cubemap;

                case SamplerType.TextureCube | SamplerType.Array:
                    return Target.CubemapArray;
            }

            Logger.Warning?.Print(LogClass.Gpu, $"Invalid sampler type \"{type}\".");

            return Target.Texture2D;
        }

        /// <summary>
        /// Gets a texture format from a shader image format.
        /// </summary>
        /// <param name="format">Shader image format</param>
        /// <returns>Texture format</returns>
        public static FormatInfo GetFormatInfo(TextureFormat format)
        {
            return format switch
            {
#pragma warning disable IDE0055 // Disable formatting
                TextureFormat.R8Unorm           => new(Format.R8Unorm, 1, 1, 1, 1),
                TextureFormat.R8Snorm           => new(Format.R8Snorm, 1, 1, 1, 1),
                TextureFormat.R8Uint            => new(Format.R8Uint, 1, 1, 1, 1),
                TextureFormat.R8Sint            => new(Format.R8Sint, 1, 1, 1, 1),
                TextureFormat.R16Float          => new(Format.R16Float, 1, 1, 2, 1),
                TextureFormat.R16Unorm          => new(Format.R16Unorm, 1, 1, 2, 1),
                TextureFormat.R16Snorm          => new(Format.R16Snorm, 1, 1, 2, 1),
                TextureFormat.R16Uint           => new(Format.R16Uint, 1, 1, 2, 1),
                TextureFormat.R16Sint           => new(Format.R16Sint, 1, 1, 2, 1),
                TextureFormat.R32Float          => new(Format.R32Float, 1, 1, 4, 1),
                TextureFormat.R32Uint           => new(Format.R32Uint, 1, 1, 4, 1),
                TextureFormat.R32Sint           => new(Format.R32Sint, 1, 1, 4, 1),
                TextureFormat.R8G8Unorm         => new(Format.R8G8Unorm, 1, 1, 2, 2),
                TextureFormat.R8G8Snorm         => new(Format.R8G8Snorm, 1, 1, 2, 2),
                TextureFormat.R8G8Uint          => new(Format.R8G8Uint, 1, 1, 2, 2),
                TextureFormat.R8G8Sint          => new(Format.R8G8Sint, 1, 1, 2, 2),
                TextureFormat.R16G16Float       => new(Format.R16G16Float, 1, 1, 4, 2),
                TextureFormat.R16G16Unorm       => new(Format.R16G16Unorm, 1, 1, 4, 2),
                TextureFormat.R16G16Snorm       => new(Format.R16G16Snorm, 1, 1, 4, 2),
                TextureFormat.R16G16Uint        => new(Format.R16G16Uint, 1, 1, 4, 2),
                TextureFormat.R16G16Sint        => new(Format.R16G16Sint, 1, 1, 4, 2),
                TextureFormat.R32G32Float       => new(Format.R32G32Float, 1, 1, 8, 2),
                TextureFormat.R32G32Uint        => new(Format.R32G32Uint, 1, 1, 8, 2),
                TextureFormat.R32G32Sint        => new(Format.R32G32Sint, 1, 1, 8, 2),
                TextureFormat.R8G8B8A8Unorm     => new(Format.R8G8B8A8Unorm, 1, 1, 4, 4),
                TextureFormat.R8G8B8A8Snorm     => new(Format.R8G8B8A8Snorm, 1, 1, 4, 4),
                TextureFormat.R8G8B8A8Uint      => new(Format.R8G8B8A8Uint, 1, 1, 4, 4),
                TextureFormat.R8G8B8A8Sint      => new(Format.R8G8B8A8Sint, 1, 1, 4, 4),
                TextureFormat.R16G16B16A16Float => new(Format.R16G16B16A16Float, 1, 1, 8, 4),
                TextureFormat.R16G16B16A16Unorm => new(Format.R16G16B16A16Unorm, 1, 1, 8, 4),
                TextureFormat.R16G16B16A16Snorm => new(Format.R16G16B16A16Snorm, 1, 1, 8, 4),
                TextureFormat.R16G16B16A16Uint  => new(Format.R16G16B16A16Uint, 1, 1, 8, 4),
                TextureFormat.R16G16B16A16Sint  => new(Format.R16G16B16A16Sint, 1, 1, 8, 4),
                TextureFormat.R32G32B32A32Float => new(Format.R32G32B32A32Float, 1, 1, 16, 4),
                TextureFormat.R32G32B32A32Uint  => new(Format.R32G32B32A32Uint, 1, 1, 16, 4),
                TextureFormat.R32G32B32A32Sint  => new(Format.R32G32B32A32Sint, 1, 1, 16, 4),
                TextureFormat.R10G10B10A2Unorm  => new(Format.R10G10B10A2Unorm, 1, 1, 4, 4),
                TextureFormat.R10G10B10A2Uint   => new(Format.R10G10B10A2Uint, 1, 1, 4, 4),
                TextureFormat.R11G11B10Float    => new(Format.R11G11B10Float, 1, 1, 4, 3),
                _                               => FormatInfo.Invalid,
#pragma warning restore IDE0055
            };
        }
    }
}

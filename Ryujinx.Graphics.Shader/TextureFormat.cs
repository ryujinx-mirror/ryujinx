using Ryujinx.Graphics.Shader.StructuredIr;

namespace Ryujinx.Graphics.Shader
{
    public enum TextureFormat
    {
        Unknown,
        R8Unorm,
        R8Snorm,
        R8Uint,
        R8Sint,
        R16Float,
        R16Unorm,
        R16Snorm,
        R16Uint,
        R16Sint,
        R32Float,
        R32Uint,
        R32Sint,
        R8G8Unorm,
        R8G8Snorm,
        R8G8Uint,
        R8G8Sint,
        R16G16Float,
        R16G16Unorm,
        R16G16Snorm,
        R16G16Uint,
        R16G16Sint,
        R32G32Float,
        R32G32Uint,
        R32G32Sint,
        R8G8B8A8Unorm,
        R8G8B8A8Snorm,
        R8G8B8A8Uint,
        R8G8B8A8Sint,
        R16G16B16A16Float,
        R16G16B16A16Unorm,
        R16G16B16A16Snorm,
        R16G16B16A16Uint,
        R16G16B16A16Sint,
        R32G32B32A32Float,
        R32G32B32A32Uint,
        R32G32B32A32Sint,
        R10G10B10A2Unorm,
        R10G10B10A2Uint,
        R11G11B10Float
    }

    static class TextureFormatExtensions
    {
        public static string ToGlslFormat(this TextureFormat format)
        {
            return format switch
            {
                TextureFormat.R8Unorm           => "r8",
                TextureFormat.R8Snorm           => "r8_snorm",
                TextureFormat.R8Uint            => "r8ui",
                TextureFormat.R8Sint            => "r8i",
                TextureFormat.R16Float          => "r16f",
                TextureFormat.R16Unorm          => "r16",
                TextureFormat.R16Snorm          => "r16_snorm",
                TextureFormat.R16Uint           => "r16ui",
                TextureFormat.R16Sint           => "r16i",
                TextureFormat.R32Float          => "r32f",
                TextureFormat.R32Uint           => "r32ui",
                TextureFormat.R32Sint           => "r32i",
                TextureFormat.R8G8Unorm         => "rg8",
                TextureFormat.R8G8Snorm         => "rg8_snorm",
                TextureFormat.R8G8Uint          => "rg8ui",
                TextureFormat.R8G8Sint          => "rg8i",
                TextureFormat.R16G16Float       => "rg16f",
                TextureFormat.R16G16Unorm       => "rg16",
                TextureFormat.R16G16Snorm       => "rg16_snorm",
                TextureFormat.R16G16Uint        => "rg16ui",
                TextureFormat.R16G16Sint        => "rg16i",
                TextureFormat.R32G32Float       => "rg32f",
                TextureFormat.R32G32Uint        => "rg32ui",
                TextureFormat.R32G32Sint        => "rg32i",
                TextureFormat.R8G8B8A8Unorm     => "rgba8",
                TextureFormat.R8G8B8A8Snorm     => "rgba8_snorm",
                TextureFormat.R8G8B8A8Uint      => "rgba8ui",
                TextureFormat.R8G8B8A8Sint      => "rgba8i",
                TextureFormat.R16G16B16A16Float => "rgba16f",
                TextureFormat.R16G16B16A16Unorm => "rgba16",
                TextureFormat.R16G16B16A16Snorm => "rgba16_snorm",
                TextureFormat.R16G16B16A16Uint  => "rgba16ui",
                TextureFormat.R16G16B16A16Sint  => "rgba16i",
                TextureFormat.R32G32B32A32Float => "rgba32f",
                TextureFormat.R32G32B32A32Uint  => "rgba32ui",
                TextureFormat.R32G32B32A32Sint  => "rgba32i",
                TextureFormat.R10G10B10A2Unorm  => "rgb10_a2",
                TextureFormat.R10G10B10A2Uint   => "rgb10_a2ui",
                TextureFormat.R11G11B10Float    => "r11f_g11f_b10f",
                _                               => string.Empty
            };
        }

        public static VariableType GetComponentType(this TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.R8Uint:
                case TextureFormat.R16Uint:
                case TextureFormat.R32Uint:
                case TextureFormat.R8G8Uint:
                case TextureFormat.R16G16Uint:
                case TextureFormat.R32G32Uint:
                case TextureFormat.R8G8B8A8Uint:
                case TextureFormat.R16G16B16A16Uint:
                case TextureFormat.R32G32B32A32Uint:
                case TextureFormat.R10G10B10A2Uint:
                    return VariableType.U32;
                case TextureFormat.R8Sint:
                case TextureFormat.R16Sint:
                case TextureFormat.R32Sint:
                case TextureFormat.R8G8Sint:
                case TextureFormat.R16G16Sint:
                case TextureFormat.R32G32Sint:
                case TextureFormat.R8G8B8A8Sint:
                case TextureFormat.R16G16B16A16Sint:
                case TextureFormat.R32G32B32A32Sint:
                    return VariableType.S32;
            };

            return VariableType.F32;
        }
    }
}

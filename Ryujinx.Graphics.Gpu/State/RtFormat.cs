using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;

namespace Ryujinx.Graphics.Gpu.State
{
    enum RtFormat
    {
        D32Float          = 0xa,
        D16Unorm          = 0x13,
        D24UnormS8Uint    = 0x14,
        S8Uint            = 0x17,
        D32FloatS8Uint    = 0x19,
        R32G32B32A32Float = 0xc0,
        R32G32B32A32Sint  = 0xc1,
        R32G32B32A32Uint  = 0xc2,
        R32G32B32X32Float = 0xc3,
        R32G32B32X32Sint  = 0xc4,
        R32G32B32X32Uint  = 0xc5,
        R16G16B16X16Unorm = 0xc6,
        R16G16B16X16Snorm = 0xc7,
        R16G16B16X16Sint  = 0xc8,
        R16G16B16X16Uint  = 0xc9,
        R16G16B16A16Float = 0xca,
        R32G32Float       = 0xcb,
        R32G32Sint        = 0xcc,
        R32G32Uint        = 0xcd,
        R16G16B16X16Float = 0xce,
        B8G8R8A8Unorm     = 0xcf,
        B8G8R8A8Srgb      = 0xd0,
        R10G10B10A2Unorm  = 0xd1,
        R10G10B10A2Uint   = 0xd2,
        R8G8B8A8Unorm     = 0xd5,
        R8G8B8A8Srgb      = 0xd6,
        R8G8B8X8Snorm     = 0xd7,
        R8G8B8X8Sint      = 0xd8,
        R8G8B8X8Uint      = 0xd9,
        R16G16Unorm       = 0xda,
        R16G16Snorm       = 0xdb,
        R16G16Sint        = 0xdc,
        R16G16Uint        = 0xdd,
        R16G16Float       = 0xde,
        R11G11B10Float    = 0xe0,
        R32Sint           = 0xe3,
        R32Uint           = 0xe4,
        R32Float          = 0xe5,
        B8G8R8X8Unorm     = 0xe6,
        B8G8R8X8Srgb      = 0xe7,
        B5G6R5Unorm       = 0xe8,
        B5G5R5A1Unorm     = 0xe9,
        R8G8Unorm         = 0xea,
        R8G8Snorm         = 0xeb,
        R8G8Sint          = 0xec,
        R8G8Uint          = 0xed,
        R16Unorm          = 0xee,
        R16Snorm          = 0xef,
        R16Sint           = 0xf0,
        R16Uint           = 0xf1,
        R16Float          = 0xf2,
        R8Unorm           = 0xf3,
        R8Snorm           = 0xf4,
        R8Sint            = 0xf5,
        R8Uint            = 0xf6,
        B5G5R5X1Unorm     = 0xf8,
        R8G8B8X8Unorm     = 0xf9,
        R8G8B8X8Srgb      = 0xfa
    }

    static class RtFormatConverter
    {
        public static FormatInfo Convert(this RtFormat format)
        {
            switch (format)
            {
                case RtFormat.D32Float:             return new FormatInfo(Format.D32Float,          1, 1, 4);
                case RtFormat.D16Unorm:             return new FormatInfo(Format.D16Unorm,          1, 1, 2);
                case RtFormat.D24UnormS8Uint:       return new FormatInfo(Format.D24UnormS8Uint,    1, 1, 4);
                case RtFormat.S8Uint:               return new FormatInfo(Format.S8Uint,            1, 1, 1);
                case RtFormat.D32FloatS8Uint:       return new FormatInfo(Format.D32FloatS8Uint,    1, 1, 8);
                case RtFormat.R32G32B32A32Float:    return new FormatInfo(Format.R32G32B32A32Float, 1, 1, 16);
                case RtFormat.R32G32B32A32Sint:     return new FormatInfo(Format.R32G32B32A32Sint,  1, 1, 16);
                case RtFormat.R32G32B32A32Uint:     return new FormatInfo(Format.R32G32B32A32Uint,  1, 1, 16);
                case RtFormat.R32G32B32X32Float:    return new FormatInfo(Format.R32G32B32A32Float, 1, 1, 16);
                case RtFormat.R32G32B32X32Sint:     return new FormatInfo(Format.R32G32B32A32Sint,  1, 1, 16);
                case RtFormat.R32G32B32X32Uint:     return new FormatInfo(Format.R32G32B32A32Uint,  1, 1, 16);
                case RtFormat.R16G16B16X16Unorm:    return new FormatInfo(Format.R16G16B16A16Unorm, 1, 1, 8);
                case RtFormat.R16G16B16X16Snorm:    return new FormatInfo(Format.R16G16B16A16Snorm, 1, 1, 8);
                case RtFormat.R16G16B16X16Sint:     return new FormatInfo(Format.R16G16B16A16Sint,  1, 1, 8);
                case RtFormat.R16G16B16X16Uint:     return new FormatInfo(Format.R16G16B16A16Uint,  1, 1, 8);
                case RtFormat.R16G16B16A16Float:    return new FormatInfo(Format.R16G16B16A16Float, 1, 1, 8);
                case RtFormat.R32G32Float:          return new FormatInfo(Format.R32G32Float,       1, 1, 8);
                case RtFormat.R32G32Sint:           return new FormatInfo(Format.R32G32Sint,        1, 1, 8);
                case RtFormat.R32G32Uint:           return new FormatInfo(Format.R32G32Uint,        1, 1, 8);
                case RtFormat.R16G16B16X16Float:    return new FormatInfo(Format.R16G16B16A16Float, 1, 1, 8);
                case RtFormat.B8G8R8A8Unorm:        return new FormatInfo(Format.B8G8R8A8Unorm,     1, 1, 4);
                case RtFormat.B8G8R8A8Srgb:         return new FormatInfo(Format.B8G8R8A8Srgb,      1, 1, 4);
                case RtFormat.R10G10B10A2Unorm:     return new FormatInfo(Format.R10G10B10A2Unorm,  1, 1, 4);
                case RtFormat.R10G10B10A2Uint:      return new FormatInfo(Format.R10G10B10A2Uint,   1, 1, 4);
                case RtFormat.R8G8B8A8Unorm:        return new FormatInfo(Format.R8G8B8A8Unorm,     1, 1, 4);
                case RtFormat.R8G8B8A8Srgb:         return new FormatInfo(Format.R8G8B8A8Srgb,      1, 1, 4);
                case RtFormat.R8G8B8X8Snorm:        return new FormatInfo(Format.R8G8B8A8Snorm,     1, 1, 4);
                case RtFormat.R8G8B8X8Sint:         return new FormatInfo(Format.R8G8B8A8Sint,      1, 1, 4);
                case RtFormat.R8G8B8X8Uint:         return new FormatInfo(Format.R8G8B8A8Uint,      1, 1, 4);
                case RtFormat.R16G16Unorm:          return new FormatInfo(Format.R16G16Unorm,       1, 1, 4);
                case RtFormat.R16G16Snorm:          return new FormatInfo(Format.R16G16Snorm,       1, 1, 4);
                case RtFormat.R16G16Sint:           return new FormatInfo(Format.R16G16Sint,        1, 1, 4);
                case RtFormat.R16G16Uint:           return new FormatInfo(Format.R16G16Uint,        1, 1, 4);
                case RtFormat.R16G16Float:          return new FormatInfo(Format.R16G16Float,       1, 1, 4);
                case RtFormat.R11G11B10Float:       return new FormatInfo(Format.R11G11B10Float,    1, 1, 4);
                case RtFormat.R32Sint:              return new FormatInfo(Format.R32Sint,           1, 1, 4);
                case RtFormat.R32Uint:              return new FormatInfo(Format.R32Uint,           1, 1, 4);
                case RtFormat.R32Float:             return new FormatInfo(Format.R32Float,          1, 1, 4);
                case RtFormat.B8G8R8X8Unorm:        return new FormatInfo(Format.B8G8R8A8Unorm,     1, 1, 4);
                case RtFormat.B8G8R8X8Srgb:         return new FormatInfo(Format.B8G8R8A8Srgb,      1, 1, 4);
                case RtFormat.B5G6R5Unorm:          return new FormatInfo(Format.B5G6R5Unorm,       1, 1, 2);
                case RtFormat.B5G5R5A1Unorm:        return new FormatInfo(Format.B5G5R5A1Unorm,     1, 1, 2);
                case RtFormat.R8G8Unorm:            return new FormatInfo(Format.R8G8Unorm,         1, 1, 2);
                case RtFormat.R8G8Snorm:            return new FormatInfo(Format.R8G8Snorm,         1, 1, 2);
                case RtFormat.R8G8Sint:             return new FormatInfo(Format.R8G8Sint,          1, 1, 2);
                case RtFormat.R8G8Uint:             return new FormatInfo(Format.R8G8Uint,          1, 1, 2);
                case RtFormat.R16Unorm:             return new FormatInfo(Format.R16Unorm,          1, 1, 2);
                case RtFormat.R16Snorm:             return new FormatInfo(Format.R16Snorm,          1, 1, 2);
                case RtFormat.R16Sint:              return new FormatInfo(Format.R16Sint,           1, 1, 2);
                case RtFormat.R16Uint:              return new FormatInfo(Format.R16Uint,           1, 1, 2);
                case RtFormat.R16Float:             return new FormatInfo(Format.R16Float,          1, 1, 2);
                case RtFormat.R8Unorm:              return new FormatInfo(Format.R8Unorm,           1, 1, 1);
                case RtFormat.R8Snorm:              return new FormatInfo(Format.R8Snorm,           1, 1, 1);
                case RtFormat.R8Sint:               return new FormatInfo(Format.R8Sint,            1, 1, 1);
                case RtFormat.R8Uint:               return new FormatInfo(Format.R8Uint,            1, 1, 1);
                case RtFormat.B5G5R5X1Unorm:        return new FormatInfo(Format.B5G5R5X1Unorm,     1, 1, 2);
                case RtFormat.R8G8B8X8Unorm:        return new FormatInfo(Format.R8G8B8A8Unorm,     1, 1, 4);
                case RtFormat.R8G8B8X8Srgb:         return new FormatInfo(Format.R8G8B8A8Srgb,      1, 1, 4);
            }

            return FormatInfo.Default;
        }
    }
}
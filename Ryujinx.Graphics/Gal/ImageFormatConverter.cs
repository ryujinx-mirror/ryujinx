using System;

namespace Ryujinx.Graphics.Gal
{
    public static class ImageFormatConverter
    {
        public static GalImageFormat ConvertTexture(
            GalTextureFormat Format,
            GalTextureType RType,
            GalTextureType GType,
            GalTextureType BType,
            GalTextureType AType)
        {
            if (RType != GType || RType != BType || RType != AType)
            {
                throw new NotImplementedException("Per component types are not implemented");
            }

            GalTextureType Type = RType;

            switch (Type)
            {
                case GalTextureType.Snorm:
                    switch (Format)
                    {
                        case GalTextureFormat.R16G16B16A16: return GalImageFormat.R16G16B16A16_SNORM;
                        case GalTextureFormat.A8B8G8R8:     return GalImageFormat.A8B8G8R8_SNORM_PACK32;
                        case GalTextureFormat.A2B10G10R10:  return GalImageFormat.A2B10G10R10_SNORM_PACK32;
                        case GalTextureFormat.G8R8:         return GalImageFormat.R8G8_SNORM;
                        case GalTextureFormat.R16:          return GalImageFormat.R16_SNORM;
                        case GalTextureFormat.R8:           return GalImageFormat.R8_SNORM;
                        case GalTextureFormat.BC4:          return GalImageFormat.BC4_SNORM_BLOCK;
                        case GalTextureFormat.BC5:          return GalImageFormat.BC5_SNORM_BLOCK;
                    }
                    break;

                case GalTextureType.Unorm:
                    switch (Format)
                    {
                        case GalTextureFormat.R16G16B16A16: return GalImageFormat.R16G16B16A16_UNORM;
                        case GalTextureFormat.A8B8G8R8:     return GalImageFormat.A8B8G8R8_UNORM_PACK32;
                        case GalTextureFormat.A2B10G10R10:  return GalImageFormat.A2B10G10R10_UNORM_PACK32;
                        case GalTextureFormat.A4B4G4R4:     return GalImageFormat.R4G4B4A4_UNORM_PACK16_REVERSED;
                        case GalTextureFormat.A1B5G5R5:     return GalImageFormat.A1R5G5B5_UNORM_PACK16;
                        case GalTextureFormat.B5G6R5:       return GalImageFormat.B5G6R5_UNORM_PACK16;
                        case GalTextureFormat.BC7U:         return GalImageFormat.BC7_UNORM_BLOCK;
                        case GalTextureFormat.G8R8:         return GalImageFormat.R8G8_UNORM;
                        case GalTextureFormat.R16:          return GalImageFormat.R16_UNORM;
                        case GalTextureFormat.R8:           return GalImageFormat.R8_UNORM;
                        case GalTextureFormat.BC1:          return GalImageFormat.BC1_RGBA_UNORM_BLOCK;
                        case GalTextureFormat.BC2:          return GalImageFormat.BC2_UNORM_BLOCK;
                        case GalTextureFormat.BC3:          return GalImageFormat.BC3_UNORM_BLOCK;
                        case GalTextureFormat.BC4:          return GalImageFormat.BC4_UNORM_BLOCK;
                        case GalTextureFormat.BC5:          return GalImageFormat.BC5_UNORM_BLOCK;
                        case GalTextureFormat.Z24S8:        return GalImageFormat.D24_UNORM_S8_UINT;
                        case GalTextureFormat.Astc2D4x4:    return GalImageFormat.ASTC_4x4_UNORM_BLOCK;
                        case GalTextureFormat.Astc2D5x5:    return GalImageFormat.ASTC_5x5_UNORM_BLOCK;
                        case GalTextureFormat.Astc2D6x6:    return GalImageFormat.ASTC_6x6_UNORM_BLOCK;
                        case GalTextureFormat.Astc2D8x8:    return GalImageFormat.ASTC_8x8_UNORM_BLOCK;
                        case GalTextureFormat.Astc2D10x10:  return GalImageFormat.ASTC_10x10_UNORM_BLOCK;
                        case GalTextureFormat.Astc2D12x12:  return GalImageFormat.ASTC_12x12_UNORM_BLOCK;
                        case GalTextureFormat.Astc2D5x4:    return GalImageFormat.ASTC_5x4_UNORM_BLOCK;
                        case GalTextureFormat.Astc2D6x5:    return GalImageFormat.ASTC_6x5_UNORM_BLOCK;
                        case GalTextureFormat.Astc2D8x6:    return GalImageFormat.ASTC_8x6_UNORM_BLOCK;
                        case GalTextureFormat.Astc2D10x8:   return GalImageFormat.ASTC_10x8_UNORM_BLOCK;
                        case GalTextureFormat.Astc2D12x10:  return GalImageFormat.ASTC_12x10_UNORM_BLOCK;
                        case GalTextureFormat.Astc2D8x5:    return GalImageFormat.ASTC_8x5_UNORM_BLOCK;
                        case GalTextureFormat.Astc2D10x5:   return GalImageFormat.ASTC_10x5_UNORM_BLOCK;
                        case GalTextureFormat.Astc2D10x6:   return GalImageFormat.ASTC_10x6_UNORM_BLOCK;
                    }
                    break;

                case GalTextureType.Sint:
                    switch (Format)
                    {
                        case GalTextureFormat.R32G32B32A32: return GalImageFormat.R32G32B32A32_SINT;
                        case GalTextureFormat.R16G16B16A16: return GalImageFormat.R16G16B16A16_SINT;
                        case GalTextureFormat.R32G32:       return GalImageFormat.R32G32_SINT;
                        case GalTextureFormat.A8B8G8R8:     return GalImageFormat.A8B8G8R8_SINT_PACK32;
                        case GalTextureFormat.A2B10G10R10:  return GalImageFormat.A2B10G10R10_SINT_PACK32;
                        case GalTextureFormat.R32:          return GalImageFormat.R32_SINT;
                        case GalTextureFormat.G8R8:         return GalImageFormat.R8G8_SINT;
                        case GalTextureFormat.R16:          return GalImageFormat.R16_SINT;
                        case GalTextureFormat.R8:           return GalImageFormat.R8_SINT;
                    }
                    break;

                case GalTextureType.Uint:
                    switch (Format)
                    {
                        case GalTextureFormat.R32G32B32A32: return GalImageFormat.R32G32B32A32_UINT;
                        case GalTextureFormat.R16G16B16A16: return GalImageFormat.R16G16B16A16_UINT;
                        case GalTextureFormat.R32G32:       return GalImageFormat.R32G32_UINT;
                        case GalTextureFormat.A8B8G8R8:     return GalImageFormat.A8B8G8R8_UINT_PACK32;
                        case GalTextureFormat.A2B10G10R10:  return GalImageFormat.A2B10G10R10_UINT_PACK32;
                        case GalTextureFormat.R32:          return GalImageFormat.R32_UINT;
                        case GalTextureFormat.G8R8:         return GalImageFormat.R8G8_UINT;
                        case GalTextureFormat.R16:          return GalImageFormat.R16_UINT;
                        case GalTextureFormat.R8:           return GalImageFormat.R8_UINT;
                    }
                    break;

                case GalTextureType.Snorm_Force_Fp16:
                    //TODO
                    break;

                case GalTextureType.Unorm_Force_Fp16:
                    //TODO
                    break;

                case GalTextureType.Float:
                    switch (Format)
                    {
                        case GalTextureFormat.R32G32B32A32: return GalImageFormat.R32G32B32A32_SFLOAT;
                        case GalTextureFormat.R16G16B16A16: return GalImageFormat.R16G16B16A16_SFLOAT;
                        case GalTextureFormat.R32G32:       return GalImageFormat.R32G32_SFLOAT;
                        case GalTextureFormat.R32:          return GalImageFormat.R32_SFLOAT;
                        case GalTextureFormat.BC6H_SF16:    return GalImageFormat.BC6H_SFLOAT_BLOCK;
                        case GalTextureFormat.BC6H_UF16:    return GalImageFormat.BC6H_UFLOAT_BLOCK;
                        case GalTextureFormat.R16:          return GalImageFormat.R16_SFLOAT;
                        case GalTextureFormat.BF10GF11RF11: return GalImageFormat.B10G11R11_UFLOAT_PACK32;
                        case GalTextureFormat.ZF32:         return GalImageFormat.D32_SFLOAT;
                    }
                    break;
            }

            throw new NotImplementedException("0x" + Format.ToString("x2") + " " + Type.ToString());
        }

        public static GalImageFormat ConvertFrameBuffer(GalFrameBufferFormat Format)
        {
            switch (Format)
            {
                case GalFrameBufferFormat.R32Float:       return GalImageFormat.R32_SFLOAT;
                case GalFrameBufferFormat.RGB10A2Unorm:   return GalImageFormat.A2B10G10R10_UNORM_PACK32;
                case GalFrameBufferFormat.RGBA8Srgb:      return GalImageFormat.A8B8G8R8_SRGB_PACK32;
                case GalFrameBufferFormat.RGBA16Float:    return GalImageFormat.R16G16B16A16_SFLOAT;
                case GalFrameBufferFormat.R16Float:       return GalImageFormat.R16_SFLOAT;
                case GalFrameBufferFormat.R8Unorm:        return GalImageFormat.R8_UNORM;
                case GalFrameBufferFormat.RGBA8Unorm:     return GalImageFormat.A8B8G8R8_UNORM_PACK32;
                case GalFrameBufferFormat.R11G11B10Float: return GalImageFormat.B10G11R11_UFLOAT_PACK32;
                case GalFrameBufferFormat.RGBA32Float:    return GalImageFormat.R32G32B32A32_SFLOAT;
                case GalFrameBufferFormat.RG16Snorm:      return GalImageFormat.R16G16_SNORM;
                case GalFrameBufferFormat.RG16Float:      return GalImageFormat.R16G16_SFLOAT;
                case GalFrameBufferFormat.RG8Snorm:       return GalImageFormat.R8_SNORM;
                case GalFrameBufferFormat.RGBA8Snorm:     return GalImageFormat.A8B8G8R8_SNORM_PACK32;
                case GalFrameBufferFormat.RG8Unorm:       return GalImageFormat.R8G8_UNORM;
                case GalFrameBufferFormat.RG32Float:      return GalImageFormat.R32G32_SFLOAT;
                case GalFrameBufferFormat.RG32Sint:       return GalImageFormat.R32G32_SINT;
                case GalFrameBufferFormat.RG32Uint:       return GalImageFormat.R32G32_UINT;
            }

            throw new NotImplementedException(Format.ToString());
        }

        public static GalImageFormat ConvertZeta(GalZetaFormat Format)
        {
            switch (Format)
            {
                case GalZetaFormat.Z32Float:   return GalImageFormat.D32_SFLOAT;
                case GalZetaFormat.S8Z24Unorm: return GalImageFormat.D24_UNORM_S8_UINT;
                case GalZetaFormat.Z16Unorm:   return GalImageFormat.D16_UNORM;
            }

            throw new NotImplementedException(Format.ToString());
        }

        public static bool HasColor(GalImageFormat Format)
        {
            switch (Format)
            {
                case GalImageFormat.R32G32B32A32_SFLOAT:
                case GalImageFormat.R32G32B32A32_SINT:
                case GalImageFormat.R32G32B32A32_UINT:
                case GalImageFormat.R16G16B16A16_SFLOAT:
                case GalImageFormat.R16G16B16A16_SINT:
                case GalImageFormat.R16G16B16A16_UINT:
                case GalImageFormat.R32G32_SFLOAT:
                case GalImageFormat.R32G32_SINT:
                case GalImageFormat.R32G32_UINT:
                case GalImageFormat.A8B8G8R8_SNORM_PACK32:
                case GalImageFormat.A8B8G8R8_UNORM_PACK32:
                case GalImageFormat.A8B8G8R8_SINT_PACK32:
                case GalImageFormat.A8B8G8R8_UINT_PACK32:
                case GalImageFormat.A2B10G10R10_SINT_PACK32:
                case GalImageFormat.A2B10G10R10_SNORM_PACK32:
                case GalImageFormat.A2B10G10R10_UINT_PACK32:
                case GalImageFormat.A2B10G10R10_UNORM_PACK32:
                case GalImageFormat.R32_SFLOAT:
                case GalImageFormat.R32_SINT:
                case GalImageFormat.R32_UINT:
                case GalImageFormat.BC6H_SFLOAT_BLOCK:
                case GalImageFormat.BC6H_UFLOAT_BLOCK:
                case GalImageFormat.A1R5G5B5_UNORM_PACK16:
                case GalImageFormat.B5G6R5_UNORM_PACK16:
                case GalImageFormat.BC7_UNORM_BLOCK:
                case GalImageFormat.R16G16_SFLOAT:
                case GalImageFormat.R16G16_SINT:
                case GalImageFormat.R16G16_SNORM:
                case GalImageFormat.R16G16_UNORM:
                case GalImageFormat.R8G8_SINT:
                case GalImageFormat.R8G8_SNORM:
                case GalImageFormat.R8G8_UINT:
                case GalImageFormat.R8G8_UNORM:
                case GalImageFormat.R16_SFLOAT:
                case GalImageFormat.R16_SINT:
                case GalImageFormat.R16_SNORM:
                case GalImageFormat.R16_UINT:
                case GalImageFormat.R16_UNORM:
                case GalImageFormat.R8_SINT:
                case GalImageFormat.R8_SNORM:
                case GalImageFormat.R8_UINT:
                case GalImageFormat.R8_UNORM:
                case GalImageFormat.B10G11R11_UFLOAT_PACK32:
                case GalImageFormat.BC1_RGBA_UNORM_BLOCK:
                case GalImageFormat.BC2_UNORM_BLOCK:
                case GalImageFormat.BC3_UNORM_BLOCK:
                case GalImageFormat.BC4_UNORM_BLOCK:
                case GalImageFormat.BC5_UNORM_BLOCK:
                case GalImageFormat.ASTC_4x4_UNORM_BLOCK:
                case GalImageFormat.ASTC_5x5_UNORM_BLOCK:
                case GalImageFormat.ASTC_6x6_UNORM_BLOCK:
                case GalImageFormat.ASTC_8x8_UNORM_BLOCK:
                case GalImageFormat.ASTC_10x10_UNORM_BLOCK:
                case GalImageFormat.ASTC_12x12_UNORM_BLOCK:
                case GalImageFormat.ASTC_5x4_UNORM_BLOCK:
                case GalImageFormat.ASTC_6x5_UNORM_BLOCK:
                case GalImageFormat.ASTC_8x6_UNORM_BLOCK:
                case GalImageFormat.ASTC_10x8_UNORM_BLOCK:
                case GalImageFormat.ASTC_12x10_UNORM_BLOCK:
                case GalImageFormat.ASTC_8x5_UNORM_BLOCK:
                case GalImageFormat.ASTC_10x5_UNORM_BLOCK:
                case GalImageFormat.ASTC_10x6_UNORM_BLOCK:
                case GalImageFormat.R4G4B4A4_UNORM_PACK16_REVERSED:
                    return true;

                case GalImageFormat.D24_UNORM_S8_UINT:
                case GalImageFormat.D32_SFLOAT:
                case GalImageFormat.D16_UNORM:
                    return true;
            }

            throw new NotImplementedException(Format.ToString());
        }

        public static bool HasDepth(GalImageFormat Format)
        {
            switch (Format)
            {
                case GalImageFormat.D24_UNORM_S8_UINT:
                case GalImageFormat.D32_SFLOAT:
                case GalImageFormat.D16_UNORM:
                    return true;
            }

            //Depth formats are fewer than colors, so it's harder to miss one
            //Instead of checking for individual formats, return false
            return false;
        }

        public static bool HasStencil(GalImageFormat Format)
        {
            switch (Format)
            {
                case GalImageFormat.D24_UNORM_S8_UINT:
                    return true;
            }

            return false;
        }
    }
}
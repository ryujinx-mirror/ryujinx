using Ryujinx.Graphics.GAL;
using System;
using VkFormat = Silk.NET.Vulkan.Format;

namespace Ryujinx.Graphics.Vulkan
{
    static class FormatTable
    {
        private static readonly VkFormat[] Table;

        static FormatTable()
        {
            Table = new VkFormat[Enum.GetNames(typeof(Format)).Length];

            Add(Format.R8Unorm,             VkFormat.R8Unorm);
            Add(Format.R8Snorm,             VkFormat.R8SNorm);
            Add(Format.R8Uint,              VkFormat.R8Uint);
            Add(Format.R8Sint,              VkFormat.R8Sint);
            Add(Format.R16Float,            VkFormat.R16Sfloat);
            Add(Format.R16Unorm,            VkFormat.R16Unorm);
            Add(Format.R16Snorm,            VkFormat.R16SNorm);
            Add(Format.R16Uint,             VkFormat.R16Uint);
            Add(Format.R16Sint,             VkFormat.R16Sint);
            Add(Format.R32Float,            VkFormat.R32Sfloat);
            Add(Format.R32Uint,             VkFormat.R32Uint);
            Add(Format.R32Sint,             VkFormat.R32Sint);
            Add(Format.R8G8Unorm,           VkFormat.R8G8Unorm);
            Add(Format.R8G8Snorm,           VkFormat.R8G8SNorm);
            Add(Format.R8G8Uint,            VkFormat.R8G8Uint);
            Add(Format.R8G8Sint,            VkFormat.R8G8Sint);
            Add(Format.R16G16Float,         VkFormat.R16G16Sfloat);
            Add(Format.R16G16Unorm,         VkFormat.R16G16Unorm);
            Add(Format.R16G16Snorm,         VkFormat.R16G16SNorm);
            Add(Format.R16G16Uint,          VkFormat.R16G16Uint);
            Add(Format.R16G16Sint,          VkFormat.R16G16Sint);
            Add(Format.R32G32Float,         VkFormat.R32G32Sfloat);
            Add(Format.R32G32Uint,          VkFormat.R32G32Uint);
            Add(Format.R32G32Sint,          VkFormat.R32G32Sint);
            Add(Format.R8G8B8Unorm,         VkFormat.R8G8B8Unorm);
            Add(Format.R8G8B8Snorm,         VkFormat.R8G8B8SNorm);
            Add(Format.R8G8B8Uint,          VkFormat.R8G8B8Uint);
            Add(Format.R8G8B8Sint,          VkFormat.R8G8B8Sint);
            Add(Format.R16G16B16Float,      VkFormat.R16G16B16Sfloat);
            Add(Format.R16G16B16Unorm,      VkFormat.R16G16B16Unorm);
            Add(Format.R16G16B16Snorm,      VkFormat.R16G16B16SNorm);
            Add(Format.R16G16B16Uint,       VkFormat.R16G16B16Uint);
            Add(Format.R16G16B16Sint,       VkFormat.R16G16B16Sint);
            Add(Format.R32G32B32Float,      VkFormat.R32G32B32Sfloat);
            Add(Format.R32G32B32Uint,       VkFormat.R32G32B32Uint);
            Add(Format.R32G32B32Sint,       VkFormat.R32G32B32Sint);
            Add(Format.R8G8B8A8Unorm,       VkFormat.R8G8B8A8Unorm);
            Add(Format.R8G8B8A8Snorm,       VkFormat.R8G8B8A8SNorm);
            Add(Format.R8G8B8A8Uint,        VkFormat.R8G8B8A8Uint);
            Add(Format.R8G8B8A8Sint,        VkFormat.R8G8B8A8Sint);
            Add(Format.R16G16B16A16Float,   VkFormat.R16G16B16A16Sfloat);
            Add(Format.R16G16B16A16Unorm,   VkFormat.R16G16B16A16Unorm);
            Add(Format.R16G16B16A16Snorm,   VkFormat.R16G16B16A16SNorm);
            Add(Format.R16G16B16A16Uint,    VkFormat.R16G16B16A16Uint);
            Add(Format.R16G16B16A16Sint,    VkFormat.R16G16B16A16Sint);
            Add(Format.R32G32B32A32Float,   VkFormat.R32G32B32A32Sfloat);
            Add(Format.R32G32B32A32Uint,    VkFormat.R32G32B32A32Uint);
            Add(Format.R32G32B32A32Sint,    VkFormat.R32G32B32A32Sint);
            Add(Format.S8Uint,              VkFormat.S8Uint);
            Add(Format.D16Unorm,            VkFormat.D16Unorm);
            Add(Format.S8UintD24Unorm,      VkFormat.D24UnormS8Uint);
            Add(Format.D32Float,            VkFormat.D32Sfloat);
            Add(Format.D24UnormS8Uint,      VkFormat.D24UnormS8Uint);
            Add(Format.D32FloatS8Uint,      VkFormat.D32SfloatS8Uint);
            Add(Format.R8G8B8X8Srgb,        VkFormat.R8G8B8Srgb);
            Add(Format.R8G8B8A8Srgb,        VkFormat.R8G8B8A8Srgb);
            Add(Format.R4G4Unorm,           VkFormat.R4G4UnormPack8);
            Add(Format.R4G4B4A4Unorm,       VkFormat.R4G4B4A4UnormPack16);
            Add(Format.R5G5B5X1Unorm,       VkFormat.A1R5G5B5UnormPack16);
            Add(Format.R5G5B5A1Unorm,       VkFormat.A1R5G5B5UnormPack16);
            Add(Format.R5G6B5Unorm,         VkFormat.R5G6B5UnormPack16);
            Add(Format.R10G10B10A2Unorm,    VkFormat.A2B10G10R10UnormPack32);
            Add(Format.R10G10B10A2Uint,     VkFormat.A2B10G10R10UintPack32);
            Add(Format.R11G11B10Float,      VkFormat.B10G11R11UfloatPack32);
            Add(Format.R9G9B9E5Float,       VkFormat.E5B9G9R9UfloatPack32);
            Add(Format.Bc1RgbaUnorm,        VkFormat.BC1RgbaUnormBlock);
            Add(Format.Bc2Unorm,            VkFormat.BC2UnormBlock);
            Add(Format.Bc3Unorm,            VkFormat.BC3UnormBlock);
            Add(Format.Bc1RgbaSrgb,         VkFormat.BC1RgbaSrgbBlock);
            Add(Format.Bc2Srgb,             VkFormat.BC2SrgbBlock);
            Add(Format.Bc3Srgb,             VkFormat.BC3SrgbBlock);
            Add(Format.Bc4Unorm,            VkFormat.BC4UnormBlock);
            Add(Format.Bc4Snorm,            VkFormat.BC4SNormBlock);
            Add(Format.Bc5Unorm,            VkFormat.BC5UnormBlock);
            Add(Format.Bc5Snorm,            VkFormat.BC5SNormBlock);
            Add(Format.Bc7Unorm,            VkFormat.BC7UnormBlock);
            Add(Format.Bc7Srgb,             VkFormat.BC7SrgbBlock);
            Add(Format.Bc6HSfloat,          VkFormat.BC6HSfloatBlock);
            Add(Format.Bc6HUfloat,          VkFormat.BC6HUfloatBlock);
            Add(Format.R8Uscaled,           VkFormat.R8Uscaled);
            Add(Format.R8Sscaled,           VkFormat.R8Sscaled);
            Add(Format.R16Uscaled,          VkFormat.R16Uscaled);
            Add(Format.R16Sscaled,          VkFormat.R16Sscaled);
            // Add(Format.R32Uscaled,          VkFormat.R32Uscaled);
            // Add(Format.R32Sscaled,          VkFormat.R32Sscaled);
            Add(Format.R8G8Uscaled,         VkFormat.R8G8Uscaled);
            Add(Format.R8G8Sscaled,         VkFormat.R8G8Sscaled);
            Add(Format.R16G16Uscaled,       VkFormat.R16G16Uscaled);
            Add(Format.R16G16Sscaled,       VkFormat.R16G16Sscaled);
            // Add(Format.R32G32Uscaled,       VkFormat.R32G32Uscaled);
            // Add(Format.R32G32Sscaled,       VkFormat.R32G32Sscaled);
            Add(Format.R8G8B8Uscaled,       VkFormat.R8G8B8Uscaled);
            Add(Format.R8G8B8Sscaled,       VkFormat.R8G8B8Sscaled);
            Add(Format.R16G16B16Uscaled,    VkFormat.R16G16B16Uscaled);
            Add(Format.R16G16B16Sscaled,    VkFormat.R16G16B16Sscaled);
            // Add(Format.R32G32B32Uscaled,    VkFormat.R32G32B32Uscaled);
            // Add(Format.R32G32B32Sscaled,    VkFormat.R32G32B32Sscaled);
            Add(Format.R8G8B8A8Uscaled,     VkFormat.R8G8B8A8Uscaled);
            Add(Format.R8G8B8A8Sscaled,     VkFormat.R8G8B8A8Sscaled);
            Add(Format.R16G16B16A16Uscaled, VkFormat.R16G16B16A16Uscaled);
            Add(Format.R16G16B16A16Sscaled, VkFormat.R16G16B16A16Sscaled);
            // Add(Format.R32G32B32A32Uscaled, VkFormat.R32G32B32A32Uscaled);
            // Add(Format.R32G32B32A32Sscaled, VkFormat.R32G32B32A32Sscaled);
            Add(Format.R10G10B10A2Snorm,    VkFormat.A2B10G10R10SNormPack32);
            Add(Format.R10G10B10A2Sint,     VkFormat.A2B10G10R10SintPack32);
            Add(Format.R10G10B10A2Uscaled,  VkFormat.A2B10G10R10UscaledPack32);
            Add(Format.R10G10B10A2Sscaled,  VkFormat.A2B10G10R10SscaledPack32);
            Add(Format.R8G8B8X8Unorm,       VkFormat.R8G8B8Unorm);
            Add(Format.R8G8B8X8Snorm,       VkFormat.R8G8B8SNorm);
            Add(Format.R8G8B8X8Uint,        VkFormat.R8G8B8Uint);
            Add(Format.R8G8B8X8Sint,        VkFormat.R8G8B8Sint);
            Add(Format.R16G16B16X16Float,   VkFormat.R16G16B16Sfloat);
            Add(Format.R16G16B16X16Unorm,   VkFormat.R16G16B16Unorm);
            Add(Format.R16G16B16X16Snorm,   VkFormat.R16G16B16SNorm);
            Add(Format.R16G16B16X16Uint,    VkFormat.R16G16B16Uint);
            Add(Format.R16G16B16X16Sint,    VkFormat.R16G16B16Sint);
            Add(Format.R32G32B32X32Float,   VkFormat.R32G32B32Sfloat);
            Add(Format.R32G32B32X32Uint,    VkFormat.R32G32B32Uint);
            Add(Format.R32G32B32X32Sint,    VkFormat.R32G32B32Sint);
            Add(Format.Astc4x4Unorm,        VkFormat.Astc4x4UnormBlock);
            Add(Format.Astc5x4Unorm,        VkFormat.Astc5x4UnormBlock);
            Add(Format.Astc5x5Unorm,        VkFormat.Astc5x5UnormBlock);
            Add(Format.Astc6x5Unorm,        VkFormat.Astc6x5UnormBlock);
            Add(Format.Astc6x6Unorm,        VkFormat.Astc6x6UnormBlock);
            Add(Format.Astc8x5Unorm,        VkFormat.Astc8x5UnormBlock);
            Add(Format.Astc8x6Unorm,        VkFormat.Astc8x6UnormBlock);
            Add(Format.Astc8x8Unorm,        VkFormat.Astc8x8UnormBlock);
            Add(Format.Astc10x5Unorm,       VkFormat.Astc10x5UnormBlock);
            Add(Format.Astc10x6Unorm,       VkFormat.Astc10x6UnormBlock);
            Add(Format.Astc10x8Unorm,       VkFormat.Astc10x8UnormBlock);
            Add(Format.Astc10x10Unorm,      VkFormat.Astc10x10UnormBlock);
            Add(Format.Astc12x10Unorm,      VkFormat.Astc12x10UnormBlock);
            Add(Format.Astc12x12Unorm,      VkFormat.Astc12x12UnormBlock);
            Add(Format.Astc4x4Srgb,         VkFormat.Astc4x4SrgbBlock);
            Add(Format.Astc5x4Srgb,         VkFormat.Astc5x4SrgbBlock);
            Add(Format.Astc5x5Srgb,         VkFormat.Astc5x5SrgbBlock);
            Add(Format.Astc6x5Srgb,         VkFormat.Astc6x5SrgbBlock);
            Add(Format.Astc6x6Srgb,         VkFormat.Astc6x6SrgbBlock);
            Add(Format.Astc8x5Srgb,         VkFormat.Astc8x5SrgbBlock);
            Add(Format.Astc8x6Srgb,         VkFormat.Astc8x6SrgbBlock);
            Add(Format.Astc8x8Srgb,         VkFormat.Astc8x8SrgbBlock);
            Add(Format.Astc10x5Srgb,        VkFormat.Astc10x5SrgbBlock);
            Add(Format.Astc10x6Srgb,        VkFormat.Astc10x6SrgbBlock);
            Add(Format.Astc10x8Srgb,        VkFormat.Astc10x8SrgbBlock);
            Add(Format.Astc10x10Srgb,       VkFormat.Astc10x10SrgbBlock);
            Add(Format.Astc12x10Srgb,       VkFormat.Astc12x10SrgbBlock);
            Add(Format.Astc12x12Srgb,       VkFormat.Astc12x12SrgbBlock);
            Add(Format.B5G6R5Unorm,         VkFormat.R5G6B5UnormPack16);
            Add(Format.B5G5R5X1Unorm,       VkFormat.A1R5G5B5UnormPack16);
            Add(Format.B5G5R5A1Unorm,       VkFormat.A1R5G5B5UnormPack16);
            Add(Format.A1B5G5R5Unorm,       VkFormat.R5G5B5A1UnormPack16);
            Add(Format.B8G8R8X8Unorm,       VkFormat.B8G8R8Unorm);
            Add(Format.B8G8R8A8Unorm,       VkFormat.B8G8R8A8Unorm);
            Add(Format.B8G8R8X8Srgb,        VkFormat.B8G8R8Srgb);
            Add(Format.B8G8R8A8Srgb,        VkFormat.B8G8R8A8Srgb);
        }

        private static void Add(Format format, VkFormat vkFormat)
        {
            Table[(int)format] = vkFormat;
        }

        public static VkFormat GetFormat(Format format)
        {
            return Table[(int)format];
        }
    }
}

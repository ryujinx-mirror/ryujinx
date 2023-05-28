using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Silk.NET.Vulkan;
using System;
using VkFormat = Silk.NET.Vulkan.Format;

namespace Ryujinx.Graphics.Vulkan
{
    class FormatCapabilities
    {
        private readonly FormatFeatureFlags[] _bufferTable;
        private readonly FormatFeatureFlags[] _optimalTable;

        private readonly Vk _api;
        private readonly PhysicalDevice _physicalDevice;

        public FormatCapabilities(Vk api, PhysicalDevice physicalDevice)
        {
            _api = api;
            _physicalDevice = physicalDevice;

            int totalFormats = Enum.GetNames(typeof(GAL.Format)).Length;

            _bufferTable = new FormatFeatureFlags[totalFormats];
            _optimalTable = new FormatFeatureFlags[totalFormats];
        }

        public bool BufferFormatsSupport(FormatFeatureFlags flags, params GAL.Format[] formats)
        {
            foreach (GAL.Format format in formats)
            {
                if (!BufferFormatSupports(flags, format))
                {
                    return false;
                }
            }

            return true;
        }

        public bool OptimalFormatsSupport(FormatFeatureFlags flags, params GAL.Format[] formats)
        {
            foreach (GAL.Format format in formats)
            {
                if (!OptimalFormatSupports(flags, format))
                {
                    return false;
                }
            }

            return true;
        }

        public bool BufferFormatSupports(FormatFeatureFlags flags, GAL.Format format)
        {
            var formatFeatureFlags = _bufferTable[(int)format];

            if (formatFeatureFlags == 0)
            {
                _api.GetPhysicalDeviceFormatProperties(_physicalDevice, FormatTable.GetFormat(format), out var fp);
                formatFeatureFlags = fp.BufferFeatures;
                _bufferTable[(int)format] = formatFeatureFlags;
            }

            return (formatFeatureFlags & flags) == flags;
        }

        public bool BufferFormatSupports(FormatFeatureFlags flags, VkFormat format)
        {
            _api.GetPhysicalDeviceFormatProperties(_physicalDevice, format, out var fp);

            return (fp.BufferFeatures & flags) == flags;
        }

        public bool OptimalFormatSupports(FormatFeatureFlags flags, GAL.Format format)
        {
            var formatFeatureFlags = _optimalTable[(int)format];

            if (formatFeatureFlags == 0)
            {
                _api.GetPhysicalDeviceFormatProperties(_physicalDevice, FormatTable.GetFormat(format), out var fp);
                formatFeatureFlags = fp.OptimalTilingFeatures;
                _optimalTable[(int)format] = formatFeatureFlags;
            }

            return (formatFeatureFlags & flags) == flags;
        }

        public VkFormat ConvertToVkFormat(GAL.Format srcFormat)
        {
            var format = FormatTable.GetFormat(srcFormat);

            var requiredFeatures = FormatFeatureFlags.SampledImageBit |
                                   FormatFeatureFlags.TransferSrcBit |
                                   FormatFeatureFlags.TransferDstBit;

            if (srcFormat.IsDepthOrStencil())
            {
                requiredFeatures |= FormatFeatureFlags.DepthStencilAttachmentBit;
            }
            else if (srcFormat.IsRtColorCompatible())
            {
                requiredFeatures |= FormatFeatureFlags.ColorAttachmentBit;
            }

            if (srcFormat.IsImageCompatible())
            {
                requiredFeatures |= FormatFeatureFlags.StorageImageBit;
            }

            if (!OptimalFormatSupports(requiredFeatures, srcFormat) || (IsD24S8(srcFormat) && VulkanConfiguration.ForceD24S8Unsupported))
            {
                // The format is not supported. Can we convert it to a higher precision format?
                if (IsD24S8(srcFormat))
                {
                    format = VkFormat.D32SfloatS8Uint;
                }
                else if (srcFormat == GAL.Format.R4G4B4A4Unorm)
                {
                    format = VkFormat.R4G4B4A4UnormPack16;
                }
                else
                {
                    Logger.Error?.Print(LogClass.Gpu, $"Format {srcFormat} is not supported by the host.");
                }
            }

            return format;
        }

        public VkFormat ConvertToVertexVkFormat(GAL.Format srcFormat)
        {
            var format = FormatTable.GetFormat(srcFormat);

            if (!BufferFormatSupports(FormatFeatureFlags.VertexBufferBit, srcFormat) ||
                (IsRGB16IntFloat(srcFormat) && VulkanConfiguration.ForceRGB16IntFloatUnsupported))
            {
                // The format is not supported. Can we convert it to an alternative format?
                switch (srcFormat)
                {
                    case GAL.Format.R16G16B16Float:
                        format = VkFormat.R16G16B16A16Sfloat;
                        break;
                    case GAL.Format.R16G16B16Sint:
                        format = VkFormat.R16G16B16A16Sint;
                        break;
                    case GAL.Format.R16G16B16Uint:
                        format = VkFormat.R16G16B16A16Uint;
                        break;
                    default:
                        Logger.Error?.Print(LogClass.Gpu, $"Format {srcFormat} is not supported by the host.");
                        break;
                }
            }

            return format;
        }

        public static bool IsD24S8(GAL.Format format)
        {
            return format == GAL.Format.D24UnormS8Uint || format == GAL.Format.S8UintD24Unorm;
        }

        private static bool IsRGB16IntFloat(GAL.Format format)
        {
            return format == GAL.Format.R16G16B16Float ||
                   format == GAL.Format.R16G16B16Sint ||
                   format == GAL.Format.R16G16B16Uint;
        }
    }
}

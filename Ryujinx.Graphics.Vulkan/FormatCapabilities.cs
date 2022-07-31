using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Silk.NET.Vulkan;
using System;
using VkFormat = Silk.NET.Vulkan.Format;

namespace Ryujinx.Graphics.Vulkan
{
    class FormatCapabilities
    {
        private readonly FormatFeatureFlags[] _table;

        private readonly Vk _api;
        private readonly PhysicalDevice _physicalDevice;

        public FormatCapabilities(Vk api, PhysicalDevice physicalDevice)
        {
            _api = api;
            _physicalDevice = physicalDevice;
            _table = new FormatFeatureFlags[Enum.GetNames(typeof(GAL.Format)).Length];
        }

        public bool FormatsSupports(FormatFeatureFlags flags, params GAL.Format[] formats)
        {
            foreach (GAL.Format format in formats)
            {
                if (!FormatSupports(flags, format))
                {
                    return false;
                }
            }

            return true;
        }

        public bool FormatSupports(FormatFeatureFlags flags, GAL.Format format)
        {
            var formatFeatureFlags = _table[(int)format];

            if (formatFeatureFlags == 0)
            {
                _api.GetPhysicalDeviceFormatProperties(_physicalDevice, FormatTable.GetFormat(format), out var fp);
                formatFeatureFlags = fp.OptimalTilingFeatures;
                _table[(int)format] = formatFeatureFlags;
            }

            return (formatFeatureFlags & flags) == flags;
        }

        public VkFormat ConvertToVkFormat(GAL.Format srcFormat)
        {
            var format = FormatTable.GetFormat(srcFormat);

            var requiredFeatures = FormatFeatureFlags.FormatFeatureSampledImageBit |
                                   FormatFeatureFlags.FormatFeatureTransferSrcBit |
                                   FormatFeatureFlags.FormatFeatureTransferDstBit;

            if (srcFormat.IsDepthOrStencil())
            {
                requiredFeatures |= FormatFeatureFlags.FormatFeatureDepthStencilAttachmentBit;
            }
            else if (srcFormat.IsRtColorCompatible())
            {
                requiredFeatures |= FormatFeatureFlags.FormatFeatureColorAttachmentBit;
            }

            if (srcFormat.IsImageCompatible())
            {
                requiredFeatures |= FormatFeatureFlags.FormatFeatureStorageImageBit;
            }

            if (!FormatSupports(requiredFeatures, srcFormat) || (IsD24S8(srcFormat) && VulkanConfiguration.ForceD24S8Unsupported))
            {
                // The format is not supported. Can we convert it to a higher precision format?
                if (IsD24S8(srcFormat))
                {
                    format = VkFormat.D32SfloatS8Uint;
                }
                else
                {
                    Logger.Error?.Print(LogClass.Gpu, $"Format {srcFormat} is not supported by the host.");
                }
            }

            return format;
        }

        public static bool IsD24S8(GAL.Format format)
        {
            return format == GAL.Format.D24UnormS8Uint || format == GAL.Format.S8UintD24Unorm;
        }
    }
}

using Silk.NET.Vulkan;
using System;

namespace Ryujinx.Graphics.Vulkan
{
    [Flags]
    enum PortabilitySubsetFlags
    {
        None = 0,

        VertexBufferAlignment4B = 1,
        NoTriangleFans = 1 << 1,
        NoPointMode = 1 << 2,
        No3DImageView = 1 << 3,
        NoLodBias = 1 << 4
    }

    readonly struct HardwareCapabilities
    {
        public readonly bool SupportsIndexTypeUint8;
        public readonly bool SupportsCustomBorderColor;
        public readonly bool SupportsIndirectParameters;
        public readonly bool SupportsFragmentShaderInterlock;
        public readonly bool SupportsGeometryShaderPassthrough;
        public readonly bool SupportsSubgroupSizeControl;
        public readonly bool SupportsShaderInt8;
        public readonly bool SupportsShaderStencilExport;
        public readonly bool SupportsConditionalRendering;
        public readonly bool SupportsExtendedDynamicState;
        public readonly bool SupportsMultiView;
        public readonly bool SupportsNullDescriptors;
        public readonly bool SupportsPushDescriptors;
        public readonly bool SupportsTransformFeedback;
        public readonly bool SupportsTransformFeedbackQueries;
        public readonly bool SupportsPreciseOcclusionQueries;
        public readonly bool SupportsPipelineStatisticsQuery;
        public readonly bool SupportsGeometryShader;
        public readonly uint MinSubgroupSize;
        public readonly uint MaxSubgroupSize;
        public readonly ShaderStageFlags RequiredSubgroupSizeStages;
        public readonly SampleCountFlags SupportedSampleCounts;
        public readonly PortabilitySubsetFlags PortabilitySubset;

        public HardwareCapabilities(
            bool supportsIndexTypeUint8,
            bool supportsCustomBorderColor,
            bool supportsIndirectParameters,
            bool supportsFragmentShaderInterlock,
            bool supportsGeometryShaderPassthrough,
            bool supportsSubgroupSizeControl,
            bool supportsShaderInt8,
            bool supportsShaderStencilExport,
            bool supportsConditionalRendering,
            bool supportsExtendedDynamicState,
            bool supportsMultiView,
            bool supportsNullDescriptors,
            bool supportsPushDescriptors,
            bool supportsTransformFeedback,
            bool supportsTransformFeedbackQueries,
            bool supportsPreciseOcclusionQueries,
            bool supportsPipelineStatisticsQuery,
            bool supportsGeometryShader,
            uint minSubgroupSize,
            uint maxSubgroupSize,
            ShaderStageFlags requiredSubgroupSizeStages,
            SampleCountFlags supportedSampleCounts,
            PortabilitySubsetFlags portabilitySubset)
        {
            SupportsIndexTypeUint8 = supportsIndexTypeUint8;
            SupportsCustomBorderColor = supportsCustomBorderColor;
            SupportsIndirectParameters = supportsIndirectParameters;
            SupportsFragmentShaderInterlock = supportsFragmentShaderInterlock;
            SupportsGeometryShaderPassthrough = supportsGeometryShaderPassthrough;
            SupportsSubgroupSizeControl = supportsSubgroupSizeControl;
            SupportsShaderInt8 = supportsShaderInt8;
            SupportsShaderStencilExport = supportsShaderStencilExport;
            SupportsConditionalRendering = supportsConditionalRendering;
            SupportsExtendedDynamicState = supportsExtendedDynamicState;
            SupportsMultiView = supportsMultiView;
            SupportsNullDescriptors = supportsNullDescriptors;
            SupportsPushDescriptors = supportsPushDescriptors;
            SupportsTransformFeedback = supportsTransformFeedback;
            SupportsTransformFeedbackQueries = supportsTransformFeedbackQueries;
            SupportsPreciseOcclusionQueries = supportsPreciseOcclusionQueries;
            SupportsPipelineStatisticsQuery = supportsPipelineStatisticsQuery;
            SupportsGeometryShader = supportsGeometryShader;
            MinSubgroupSize = minSubgroupSize;
            MaxSubgroupSize = maxSubgroupSize;
            RequiredSubgroupSizeStages = requiredSubgroupSizeStages;
            SupportedSampleCounts = supportedSampleCounts;
            PortabilitySubset = portabilitySubset;
        }
    }
}

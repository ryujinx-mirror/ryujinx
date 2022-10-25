using Silk.NET.Vulkan;
using System;

namespace Ryujinx.Graphics.Vulkan
{
    struct PipelineState : IDisposable
    {
        private const int RequiredSubgroupSize = 32;

        public PipelineUid Internal;

        public float LineWidth
        {
            get => BitConverter.Int32BitsToSingle((int)((Internal.Id0 >> 0) & 0xFFFFFFFF));
            set => Internal.Id0 = (Internal.Id0 & 0xFFFFFFFF00000000) | ((ulong)(uint)BitConverter.SingleToInt32Bits(value) << 0);
        }

        public float DepthBiasClamp
        {
            get => BitConverter.Int32BitsToSingle((int)((Internal.Id0 >> 32) & 0xFFFFFFFF));
            set => Internal.Id0 = (Internal.Id0 & 0xFFFFFFFF) | ((ulong)(uint)BitConverter.SingleToInt32Bits(value) << 32);
        }

        public float DepthBiasConstantFactor
        {
            get => BitConverter.Int32BitsToSingle((int)((Internal.Id1 >> 0) & 0xFFFFFFFF));
            set => Internal.Id1 = (Internal.Id1 & 0xFFFFFFFF00000000) | ((ulong)(uint)BitConverter.SingleToInt32Bits(value) << 0);
        }

        public float DepthBiasSlopeFactor
        {
            get => BitConverter.Int32BitsToSingle((int)((Internal.Id1 >> 32) & 0xFFFFFFFF));
            set => Internal.Id1 = (Internal.Id1 & 0xFFFFFFFF) | ((ulong)(uint)BitConverter.SingleToInt32Bits(value) << 32);
        }

        public uint StencilFrontCompareMask
        {
            get => (uint)((Internal.Id2 >> 0) & 0xFFFFFFFF);
            set => Internal.Id2 = (Internal.Id2 & 0xFFFFFFFF00000000) | ((ulong)value << 0);
        }

        public uint StencilFrontWriteMask
        {
            get => (uint)((Internal.Id2 >> 32) & 0xFFFFFFFF);
            set => Internal.Id2 = (Internal.Id2 & 0xFFFFFFFF) | ((ulong)value << 32);
        }

        public uint StencilFrontReference
        {
            get => (uint)((Internal.Id3 >> 0) & 0xFFFFFFFF);
            set => Internal.Id3 = (Internal.Id3 & 0xFFFFFFFF00000000) | ((ulong)value << 0);
        }

        public uint StencilBackCompareMask
        {
            get => (uint)((Internal.Id3 >> 32) & 0xFFFFFFFF);
            set => Internal.Id3 = (Internal.Id3 & 0xFFFFFFFF) | ((ulong)value << 32);
        }

        public uint StencilBackWriteMask
        {
            get => (uint)((Internal.Id4 >> 0) & 0xFFFFFFFF);
            set => Internal.Id4 = (Internal.Id4 & 0xFFFFFFFF00000000) | ((ulong)value << 0);
        }

        public uint StencilBackReference
        {
            get => (uint)((Internal.Id4 >> 32) & 0xFFFFFFFF);
            set => Internal.Id4 = (Internal.Id4 & 0xFFFFFFFF) | ((ulong)value << 32);
        }

        public float MinDepthBounds
        {
            get => BitConverter.Int32BitsToSingle((int)((Internal.Id5 >> 0) & 0xFFFFFFFF));
            set => Internal.Id5 = (Internal.Id5 & 0xFFFFFFFF00000000) | ((ulong)(uint)BitConverter.SingleToInt32Bits(value) << 0);
        }

        public float MaxDepthBounds
        {
            get => BitConverter.Int32BitsToSingle((int)((Internal.Id5 >> 32) & 0xFFFFFFFF));
            set => Internal.Id5 = (Internal.Id5 & 0xFFFFFFFF) | ((ulong)(uint)BitConverter.SingleToInt32Bits(value) << 32);
        }

        public float BlendConstantR
        {
            get => BitConverter.Int32BitsToSingle((int)((Internal.Id6 >> 0) & 0xFFFFFFFF));
            set => Internal.Id6 = (Internal.Id6 & 0xFFFFFFFF00000000) | ((ulong)(uint)BitConverter.SingleToInt32Bits(value) << 0);
        }

        public float BlendConstantG
        {
            get => BitConverter.Int32BitsToSingle((int)((Internal.Id6 >> 32) & 0xFFFFFFFF));
            set => Internal.Id6 = (Internal.Id6 & 0xFFFFFFFF) | ((ulong)(uint)BitConverter.SingleToInt32Bits(value) << 32);
        }

        public float BlendConstantB
        {
            get => BitConverter.Int32BitsToSingle((int)((Internal.Id7 >> 0) & 0xFFFFFFFF));
            set => Internal.Id7 = (Internal.Id7 & 0xFFFFFFFF00000000) | ((ulong)(uint)BitConverter.SingleToInt32Bits(value) << 0);
        }

        public float BlendConstantA
        {
            get => BitConverter.Int32BitsToSingle((int)((Internal.Id7 >> 32) & 0xFFFFFFFF));
            set => Internal.Id7 = (Internal.Id7 & 0xFFFFFFFF) | ((ulong)(uint)BitConverter.SingleToInt32Bits(value) << 32);
        }

        public PolygonMode PolygonMode
        {
            get => (PolygonMode)((Internal.Id8 >> 0) & 0x3FFFFFFF);
            set => Internal.Id8 = (Internal.Id8 & 0xFFFFFFFFC0000000) | ((ulong)value << 0);
        }

        public uint StagesCount
        {
            get => (byte)((Internal.Id8 >> 30) & 0xFF);
            set => Internal.Id8 = (Internal.Id8 & 0xFFFFFFC03FFFFFFF) | ((ulong)value << 30);
        }

        public uint VertexAttributeDescriptionsCount
        {
            get => (byte)((Internal.Id8 >> 38) & 0xFF);
            set => Internal.Id8 = (Internal.Id8 & 0xFFFFC03FFFFFFFFF) | ((ulong)value << 38);
        }

        public uint VertexBindingDescriptionsCount
        {
            get => (byte)((Internal.Id8 >> 46) & 0xFF);
            set => Internal.Id8 = (Internal.Id8 & 0xFFC03FFFFFFFFFFF) | ((ulong)value << 46);
        }

        public uint ViewportsCount
        {
            get => (byte)((Internal.Id8 >> 54) & 0xFF);
            set => Internal.Id8 = (Internal.Id8 & 0xC03FFFFFFFFFFFFF) | ((ulong)value << 54);
        }

        public uint ScissorsCount
        {
            get => (byte)((Internal.Id9 >> 0) & 0xFF);
            set => Internal.Id9 = (Internal.Id9 & 0xFFFFFFFFFFFFFF00) | ((ulong)value << 0);
        }

        public uint ColorBlendAttachmentStateCount
        {
            get => (byte)((Internal.Id9 >> 8) & 0xFF);
            set => Internal.Id9 = (Internal.Id9 & 0xFFFFFFFFFFFF00FF) | ((ulong)value << 8);
        }

        public PrimitiveTopology Topology
        {
            get => (PrimitiveTopology)((Internal.Id9 >> 16) & 0xF);
            set => Internal.Id9 = (Internal.Id9 & 0xFFFFFFFFFFF0FFFF) | ((ulong)value << 16);
        }

        public LogicOp LogicOp
        {
            get => (LogicOp)((Internal.Id9 >> 20) & 0xF);
            set => Internal.Id9 = (Internal.Id9 & 0xFFFFFFFFFF0FFFFF) | ((ulong)value << 20);
        }

        public CompareOp DepthCompareOp
        {
            get => (CompareOp)((Internal.Id9 >> 24) & 0x7);
            set => Internal.Id9 = (Internal.Id9 & 0xFFFFFFFFF8FFFFFF) | ((ulong)value << 24);
        }

        public StencilOp StencilFrontFailOp
        {
            get => (StencilOp)((Internal.Id9 >> 27) & 0x7);
            set => Internal.Id9 = (Internal.Id9 & 0xFFFFFFFFC7FFFFFF) | ((ulong)value << 27);
        }

        public StencilOp StencilFrontPassOp
        {
            get => (StencilOp)((Internal.Id9 >> 30) & 0x7);
            set => Internal.Id9 = (Internal.Id9 & 0xFFFFFFFE3FFFFFFF) | ((ulong)value << 30);
        }

        public StencilOp StencilFrontDepthFailOp
        {
            get => (StencilOp)((Internal.Id9 >> 33) & 0x7);
            set => Internal.Id9 = (Internal.Id9 & 0xFFFFFFF1FFFFFFFF) | ((ulong)value << 33);
        }

        public CompareOp StencilFrontCompareOp
        {
            get => (CompareOp)((Internal.Id9 >> 36) & 0x7);
            set => Internal.Id9 = (Internal.Id9 & 0xFFFFFF8FFFFFFFFF) | ((ulong)value << 36);
        }

        public StencilOp StencilBackFailOp
        {
            get => (StencilOp)((Internal.Id9 >> 39) & 0x7);
            set => Internal.Id9 = (Internal.Id9 & 0xFFFFFC7FFFFFFFFF) | ((ulong)value << 39);
        }

        public StencilOp StencilBackPassOp
        {
            get => (StencilOp)((Internal.Id9 >> 42) & 0x7);
            set => Internal.Id9 = (Internal.Id9 & 0xFFFFE3FFFFFFFFFF) | ((ulong)value << 42);
        }

        public StencilOp StencilBackDepthFailOp
        {
            get => (StencilOp)((Internal.Id9 >> 45) & 0x7);
            set => Internal.Id9 = (Internal.Id9 & 0xFFFF1FFFFFFFFFFF) | ((ulong)value << 45);
        }

        public CompareOp StencilBackCompareOp
        {
            get => (CompareOp)((Internal.Id9 >> 48) & 0x7);
            set => Internal.Id9 = (Internal.Id9 & 0xFFF8FFFFFFFFFFFF) | ((ulong)value << 48);
        }

        public CullModeFlags CullMode
        {
            get => (CullModeFlags)((Internal.Id9 >> 51) & 0x3);
            set => Internal.Id9 = (Internal.Id9 & 0xFFE7FFFFFFFFFFFF) | ((ulong)value << 51);
        }

        public bool PrimitiveRestartEnable
        {
            get => ((Internal.Id9 >> 53) & 0x1) != 0UL;
            set => Internal.Id9 = (Internal.Id9 & 0xFFDFFFFFFFFFFFFF) | ((value ? 1UL : 0UL) << 53);
        }

        public bool DepthClampEnable
        {
            get => ((Internal.Id9 >> 54) & 0x1) != 0UL;
            set => Internal.Id9 = (Internal.Id9 & 0xFFBFFFFFFFFFFFFF) | ((value ? 1UL : 0UL) << 54);
        }

        public bool RasterizerDiscardEnable
        {
            get => ((Internal.Id9 >> 55) & 0x1) != 0UL;
            set => Internal.Id9 = (Internal.Id9 & 0xFF7FFFFFFFFFFFFF) | ((value ? 1UL : 0UL) << 55);
        }

        public FrontFace FrontFace
        {
            get => (FrontFace)((Internal.Id9 >> 56) & 0x1);
            set => Internal.Id9 = (Internal.Id9 & 0xFEFFFFFFFFFFFFFF) | ((ulong)value << 56);
        }

        public bool DepthBiasEnable
        {
            get => ((Internal.Id9 >> 57) & 0x1) != 0UL;
            set => Internal.Id9 = (Internal.Id9 & 0xFDFFFFFFFFFFFFFF) | ((value ? 1UL : 0UL) << 57);
        }

        public bool DepthTestEnable
        {
            get => ((Internal.Id9 >> 58) & 0x1) != 0UL;
            set => Internal.Id9 = (Internal.Id9 & 0xFBFFFFFFFFFFFFFF) | ((value ? 1UL : 0UL) << 58);
        }

        public bool DepthWriteEnable
        {
            get => ((Internal.Id9 >> 59) & 0x1) != 0UL;
            set => Internal.Id9 = (Internal.Id9 & 0xF7FFFFFFFFFFFFFF) | ((value ? 1UL : 0UL) << 59);
        }

        public bool DepthBoundsTestEnable
        {
            get => ((Internal.Id9 >> 60) & 0x1) != 0UL;
            set => Internal.Id9 = (Internal.Id9 & 0xEFFFFFFFFFFFFFFF) | ((value ? 1UL : 0UL) << 60);
        }

        public bool StencilTestEnable
        {
            get => ((Internal.Id9 >> 61) & 0x1) != 0UL;
            set => Internal.Id9 = (Internal.Id9 & 0xDFFFFFFFFFFFFFFF) | ((value ? 1UL : 0UL) << 61);
        }

        public bool LogicOpEnable
        {
            get => ((Internal.Id9 >> 62) & 0x1) != 0UL;
            set => Internal.Id9 = (Internal.Id9 & 0xBFFFFFFFFFFFFFFF) | ((value ? 1UL : 0UL) << 62);
        }

        public bool HasDepthStencil
        {
            get => ((Internal.Id9 >> 63) & 0x1) != 0UL;
            set => Internal.Id9 = (Internal.Id9 & 0x7FFFFFFFFFFFFFFF) | ((value ? 1UL : 0UL) << 63);
        }

        public uint PatchControlPoints
        {
            get => (uint)((Internal.Id10 >> 0) & 0xFFFFFFFF);
            set => Internal.Id10 = (Internal.Id10 & 0xFFFFFFFF00000000) | ((ulong)value << 0);
        }

        public uint SamplesCount
        {
            get => (uint)((Internal.Id10 >> 32) & 0xFFFFFFFF);
            set => Internal.Id10 = (Internal.Id10 & 0xFFFFFFFF) | ((ulong)value << 32);
        }

        public bool AlphaToCoverageEnable
        {
            get => ((Internal.Id11 >> 0) & 0x1) != 0UL;
            set => Internal.Id11 = (Internal.Id11 & 0xFFFFFFFFFFFFFFFE) | ((value ? 1UL : 0UL) << 0);
        }

        public bool AlphaToOneEnable
        {
            get => ((Internal.Id11 >> 1) & 0x1) != 0UL;
            set => Internal.Id11 = (Internal.Id11 & 0xFFFFFFFFFFFFFFFD) | ((value ? 1UL : 0UL) << 1);
        }

        public NativeArray<PipelineShaderStageCreateInfo> Stages;
        public NativeArray<PipelineShaderStageRequiredSubgroupSizeCreateInfoEXT> StageRequiredSubgroupSizes;
        public PipelineLayout PipelineLayout;

        public void Initialize()
        {
            Stages = new NativeArray<PipelineShaderStageCreateInfo>(Constants.MaxShaderStages);
            StageRequiredSubgroupSizes = new NativeArray<PipelineShaderStageRequiredSubgroupSizeCreateInfoEXT>(Constants.MaxShaderStages);

            for (int index = 0; index < Constants.MaxShaderStages; index++)
            {
                StageRequiredSubgroupSizes[index] = new PipelineShaderStageRequiredSubgroupSizeCreateInfoEXT()
                {
                    SType = StructureType.PipelineShaderStageRequiredSubgroupSizeCreateInfoExt,
                    RequiredSubgroupSize = RequiredSubgroupSize
                };
            }
        }

        public unsafe Auto<DisposablePipeline> CreateComputePipeline(
            VulkanRenderer gd,
            Device device,
            ShaderCollection program,
            PipelineCache cache)
        {
            if (program.TryGetComputePipeline(out var pipeline))
            {
                return pipeline;
            }

            if (gd.Capabilities.SupportsSubgroupSizeControl)
            {
                UpdateStageRequiredSubgroupSizes(gd, 1);
            }

            var pipelineCreateInfo = new ComputePipelineCreateInfo()
            {
                SType = StructureType.ComputePipelineCreateInfo,
                Stage = Stages[0],
                BasePipelineIndex = -1,
                Layout = PipelineLayout
            };

            Pipeline pipelineHandle = default;

            gd.Api.CreateComputePipelines(device, cache, 1, &pipelineCreateInfo, null, &pipelineHandle).ThrowOnError();

            pipeline = new Auto<DisposablePipeline>(new DisposablePipeline(gd.Api, device, pipelineHandle));

            program.AddComputePipeline(pipeline);

            return pipeline;
        }

        public unsafe void DestroyComputePipeline(ShaderCollection program)
        {
            program.RemoveComputePipeline();
        }

        public unsafe Auto<DisposablePipeline> CreateGraphicsPipeline(
            VulkanRenderer gd,
            Device device,
            ShaderCollection program,
            PipelineCache cache,
            RenderPass renderPass)
        {
            if (program.TryGetGraphicsPipeline(ref Internal, out var pipeline))
            {
                return pipeline;
            }

            Pipeline pipelineHandle = default;

            fixed (VertexInputAttributeDescription* pVertexAttributeDescriptions = &Internal.VertexAttributeDescriptions[0])
            fixed (VertexInputBindingDescription* pVertexBindingDescriptions = &Internal.VertexBindingDescriptions[0])
            fixed (Viewport* pViewports = &Internal.Viewports[0])
            fixed (Rect2D* pScissors = &Internal.Scissors[0])
            fixed (PipelineColorBlendAttachmentState* pColorBlendAttachmentState = &Internal.ColorBlendAttachmentState[0])
            {
                var vertexInputState = new PipelineVertexInputStateCreateInfo
                {
                    SType = StructureType.PipelineVertexInputStateCreateInfo,
                    VertexAttributeDescriptionCount = VertexAttributeDescriptionsCount,
                    PVertexAttributeDescriptions = pVertexAttributeDescriptions,
                    VertexBindingDescriptionCount = VertexBindingDescriptionsCount,
                    PVertexBindingDescriptions = pVertexBindingDescriptions
                };

                bool primitiveRestartEnable = PrimitiveRestartEnable;

                primitiveRestartEnable &= Topology == PrimitiveTopology.LineStrip ||
                                          Topology == PrimitiveTopology.TriangleStrip ||
                                          Topology == PrimitiveTopology.TriangleFan ||
                                          Topology == PrimitiveTopology.LineStripWithAdjacency ||
                                          Topology == PrimitiveTopology.TriangleStripWithAdjacency;

                var inputAssemblyState = new PipelineInputAssemblyStateCreateInfo()
                {
                    SType = StructureType.PipelineInputAssemblyStateCreateInfo,
                    PrimitiveRestartEnable = primitiveRestartEnable,
                    Topology = Topology
                };

                var tessellationState = new PipelineTessellationStateCreateInfo()
                {
                    SType = StructureType.PipelineTessellationStateCreateInfo,
                    PatchControlPoints = PatchControlPoints
                };

                var rasterizationState = new PipelineRasterizationStateCreateInfo()
                {
                    SType = StructureType.PipelineRasterizationStateCreateInfo,
                    DepthClampEnable = DepthClampEnable,
                    RasterizerDiscardEnable = RasterizerDiscardEnable,
                    PolygonMode = PolygonMode,
                    LineWidth = LineWidth,
                    CullMode = CullMode,
                    FrontFace = FrontFace,
                    DepthBiasEnable = DepthBiasEnable,
                    DepthBiasClamp = DepthBiasClamp,
                    DepthBiasConstantFactor = DepthBiasConstantFactor,
                    DepthBiasSlopeFactor = DepthBiasSlopeFactor
                };

                var viewportState = new PipelineViewportStateCreateInfo()
                {
                    SType = StructureType.PipelineViewportStateCreateInfo,
                    ViewportCount = ViewportsCount,
                    PViewports = pViewports,
                    ScissorCount = ScissorsCount,
                    PScissors = pScissors
                };

                var multisampleState = new PipelineMultisampleStateCreateInfo
                {
                    SType = StructureType.PipelineMultisampleStateCreateInfo,
                    SampleShadingEnable = false,
                    RasterizationSamples = TextureStorage.ConvertToSampleCountFlags(SamplesCount),
                    MinSampleShading = 1,
                    AlphaToCoverageEnable = AlphaToCoverageEnable,
                    AlphaToOneEnable = AlphaToOneEnable
                };

                var stencilFront = new StencilOpState(
                    StencilFrontFailOp,
                    StencilFrontPassOp,
                    StencilFrontDepthFailOp,
                    StencilFrontCompareOp,
                    StencilFrontCompareMask,
                    StencilFrontWriteMask,
                    StencilFrontReference);

                var stencilBack = new StencilOpState(
                    StencilBackFailOp,
                    StencilBackPassOp,
                    StencilBackDepthFailOp,
                    StencilBackCompareOp,
                    StencilBackCompareMask,
                    StencilBackWriteMask,
                    StencilBackReference);

                var depthStencilState = new PipelineDepthStencilStateCreateInfo()
                {
                    SType = StructureType.PipelineDepthStencilStateCreateInfo,
                    DepthTestEnable = DepthTestEnable,
                    DepthWriteEnable = DepthWriteEnable,
                    DepthCompareOp = DepthCompareOp,
                    DepthBoundsTestEnable = DepthBoundsTestEnable,
                    StencilTestEnable = StencilTestEnable,
                    Front = stencilFront,
                    Back = stencilBack,
                    MinDepthBounds = MinDepthBounds,
                    MaxDepthBounds = MaxDepthBounds
                };

                var colorBlendState = new PipelineColorBlendStateCreateInfo()
                {
                    SType = StructureType.PipelineColorBlendStateCreateInfo,
                    LogicOpEnable = LogicOpEnable,
                    LogicOp = LogicOp,
                    AttachmentCount = ColorBlendAttachmentStateCount,
                    PAttachments = pColorBlendAttachmentState
                };

                colorBlendState.BlendConstants[0] = BlendConstantR;
                colorBlendState.BlendConstants[1] = BlendConstantG;
                colorBlendState.BlendConstants[2] = BlendConstantB;
                colorBlendState.BlendConstants[3] = BlendConstantA;

                bool supportsExtDynamicState = gd.Capabilities.SupportsExtendedDynamicState;
                int dynamicStatesCount = supportsExtDynamicState ? 9 : 8;

                DynamicState* dynamicStates = stackalloc DynamicState[dynamicStatesCount];

                dynamicStates[0] = DynamicState.Viewport;
                dynamicStates[1] = DynamicState.Scissor;
                dynamicStates[2] = DynamicState.DepthBias;
                dynamicStates[3] = DynamicState.DepthBounds;
                dynamicStates[4] = DynamicState.StencilCompareMask;
                dynamicStates[5] = DynamicState.StencilWriteMask;
                dynamicStates[6] = DynamicState.StencilReference;
                dynamicStates[7] = DynamicState.BlendConstants;

                if (supportsExtDynamicState)
                {
                    dynamicStates[8] = DynamicState.VertexInputBindingStrideExt;
                }

                var pipelineDynamicStateCreateInfo = new PipelineDynamicStateCreateInfo()
                {
                    SType = StructureType.PipelineDynamicStateCreateInfo,
                    DynamicStateCount = (uint)dynamicStatesCount,
                    PDynamicStates = dynamicStates
                };

                if (gd.Capabilities.SupportsSubgroupSizeControl)
                {
                    UpdateStageRequiredSubgroupSizes(gd, (int)StagesCount);
                }

                var pipelineCreateInfo = new GraphicsPipelineCreateInfo()
                {
                    SType = StructureType.GraphicsPipelineCreateInfo,
                    StageCount = StagesCount,
                    PStages = Stages.Pointer,
                    PVertexInputState = &vertexInputState,
                    PInputAssemblyState = &inputAssemblyState,
                    PTessellationState = &tessellationState,
                    PViewportState = &viewportState,
                    PRasterizationState = &rasterizationState,
                    PMultisampleState = &multisampleState,
                    PDepthStencilState = &depthStencilState,
                    PColorBlendState = &colorBlendState,
                    PDynamicState = &pipelineDynamicStateCreateInfo,
                    Layout = PipelineLayout,
                    RenderPass = renderPass,
                    BasePipelineIndex = -1
                };

                gd.Api.CreateGraphicsPipelines(device, cache, 1, &pipelineCreateInfo, null, &pipelineHandle).ThrowOnError();
            }

            pipeline = new Auto<DisposablePipeline>(new DisposablePipeline(gd.Api, device, pipelineHandle));

            program.AddGraphicsPipeline(ref Internal, pipeline);

            return pipeline;
        }

        private unsafe void UpdateStageRequiredSubgroupSizes(VulkanRenderer gd, int count)
        {
            for (int index = 0; index < count; index++)
            {
                bool canUseExplicitSubgroupSize =
                    (gd.Capabilities.RequiredSubgroupSizeStages & Stages[index].Stage) != 0 &&
                    gd.Capabilities.MinSubgroupSize <= RequiredSubgroupSize &&
                    gd.Capabilities.MaxSubgroupSize >= RequiredSubgroupSize;

                Stages[index].PNext = canUseExplicitSubgroupSize ? StageRequiredSubgroupSizes.Pointer + index : null;
            }
        }

        public void Dispose()
        {
            Stages.Dispose();
            StageRequiredSubgroupSizes.Dispose();
        }
    }
}

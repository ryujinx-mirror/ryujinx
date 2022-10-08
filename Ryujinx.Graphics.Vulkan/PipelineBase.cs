using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using Silk.NET.Vulkan;
using System;
using System.Numerics;

namespace Ryujinx.Graphics.Vulkan
{
    class PipelineBase : IDisposable
    {
        public const int DescriptorSetLayouts = 4;

        public const int UniformSetIndex = 0;
        public const int StorageSetIndex = 1;
        public const int TextureSetIndex = 2;
        public const int ImageSetIndex = 3;

        protected readonly VulkanRenderer Gd;
        protected readonly Device Device;
        public readonly PipelineCache PipelineCache;

        protected readonly AutoFlushCounter AutoFlush;

        protected PipelineDynamicState DynamicState;
        private PipelineState _newState;
        private bool _stateDirty;
        private GAL.PrimitiveTopology _topology;

        private ulong _currentPipelineHandle;

        protected Auto<DisposablePipeline> Pipeline;

        protected PipelineBindPoint Pbp;

        protected CommandBufferScoped Cbs;
        protected CommandBufferScoped? PreloadCbs;
        protected CommandBuffer CommandBuffer;

        public CommandBufferScoped CurrentCommandBuffer => Cbs;

        private ShaderCollection _program;

        private Vector4<float>[] _renderScale = new Vector4<float>[73];
        private int _fragmentScaleCount;

        protected FramebufferParams FramebufferParams;
        private Auto<DisposableFramebuffer> _framebuffer;
        private Auto<DisposableRenderPass> _renderPass;
        private int _writtenAttachmentCount;
        private bool _renderPassActive;

        private readonly DescriptorSetUpdater _descriptorSetUpdater;

        private IndexBufferState _indexBuffer;
        private IndexBufferPattern _indexBufferPattern;
        private readonly BufferState[] _transformFeedbackBuffers;
        private readonly VertexBufferState[] _vertexBuffers;
        private ulong _vertexBuffersDirty;
        protected Rectangle<int> ClearScissor;

        public SupportBufferUpdater SupportBufferUpdater;
        public IndexBufferPattern QuadsToTrisPattern;
        public IndexBufferPattern TriFanToTrisPattern;

        private bool _needsIndexBufferRebind;
        private bool _needsTransformFeedbackBuffersRebind;

        private bool _tfEnabled;
        private bool _tfActive;

        private PipelineColorBlendAttachmentState[] _storedBlend;

        public ulong DrawCount { get; private set; }

        public unsafe PipelineBase(VulkanRenderer gd, Device device)
        {
            Gd = gd;
            Device = device;

            AutoFlush = new AutoFlushCounter();

            var pipelineCacheCreateInfo = new PipelineCacheCreateInfo()
            {
                SType = StructureType.PipelineCacheCreateInfo
            };

            gd.Api.CreatePipelineCache(device, pipelineCacheCreateInfo, null, out PipelineCache).ThrowOnError();

            _descriptorSetUpdater = new DescriptorSetUpdater(gd, this);

            _transformFeedbackBuffers = new BufferState[Constants.MaxTransformFeedbackBuffers];
            _vertexBuffers = new VertexBufferState[Constants.MaxVertexBuffers + 1];

            const int EmptyVbSize = 16;

            using var emptyVb = gd.BufferManager.Create(gd, EmptyVbSize);
            emptyVb.SetData(0, new byte[EmptyVbSize]);
            _vertexBuffers[0] = new VertexBufferState(emptyVb.GetBuffer(), 0, EmptyVbSize, 0);
            _vertexBuffersDirty = ulong.MaxValue >> (64 - _vertexBuffers.Length);

            ClearScissor = new Rectangle<int>(0, 0, 0xffff, 0xffff);

            var defaultScale = new Vector4<float> { X = 1f, Y = 0f, Z = 0f, W = 0f };
            new Span<Vector4<float>>(_renderScale).Fill(defaultScale);

            _newState.Initialize();
            _newState.LineWidth = 1f;
            _newState.SamplesCount = 1;

            _storedBlend = new PipelineColorBlendAttachmentState[8];
        }

        public void Initialize()
        {
            SupportBufferUpdater = new SupportBufferUpdater(Gd);
            SupportBufferUpdater.UpdateRenderScale(_renderScale, 0, SupportBuffer.RenderScaleMaxCount);

            QuadsToTrisPattern = new IndexBufferPattern(Gd, 4, 6, 0, new[] { 0, 1, 2, 0, 2, 3 }, 4, false);
            TriFanToTrisPattern = new IndexBufferPattern(Gd, 3, 3, 2, new[] { int.MinValue, -1, 0 }, 1, true);
        }

        public unsafe void Barrier()
        {
            MemoryBarrier memoryBarrier = new MemoryBarrier()
            {
                SType = StructureType.MemoryBarrier,
                SrcAccessMask = AccessFlags.AccessMemoryReadBit | AccessFlags.AccessMemoryWriteBit,
                DstAccessMask = AccessFlags.AccessMemoryReadBit | AccessFlags.AccessMemoryWriteBit
            };

            Gd.Api.CmdPipelineBarrier(
                CommandBuffer,
                PipelineStageFlags.PipelineStageFragmentShaderBit,
                PipelineStageFlags.PipelineStageFragmentShaderBit,
                0,
                1,
                memoryBarrier,
                0,
                null,
                0,
                null);
        }

        public void BeginTransformFeedback(GAL.PrimitiveTopology topology)
        {
            _tfEnabled = true;
        }

        public void ClearBuffer(BufferHandle destination, int offset, int size, uint value)
        {
            EndRenderPass();

            var dst = Gd.BufferManager.GetBuffer(CommandBuffer, destination, offset, size, true).Get(Cbs, offset, size).Value;

            BufferHolder.InsertBufferBarrier(
                Gd,
                Cbs.CommandBuffer,
                dst,
                BufferHolder.DefaultAccessFlags,
                AccessFlags.AccessTransferWriteBit,
                PipelineStageFlags.PipelineStageAllCommandsBit,
                PipelineStageFlags.PipelineStageTransferBit,
                offset,
                size);

            Gd.Api.CmdFillBuffer(CommandBuffer, dst, (ulong)offset, (ulong)size, value);

            BufferHolder.InsertBufferBarrier(
                Gd,
                Cbs.CommandBuffer,
                dst,
                AccessFlags.AccessTransferWriteBit,
                BufferHolder.DefaultAccessFlags,
                PipelineStageFlags.PipelineStageTransferBit,
                PipelineStageFlags.PipelineStageAllCommandsBit,
                offset,
                size);
        }

        public unsafe void ClearRenderTargetColor(int index, int layer, int layerCount, ColorF color)
        {
            if (FramebufferParams == null || !FramebufferParams.IsValidColorAttachment(index))
            {
                return;
            }

            if (_renderPass == null)
            {
                CreateRenderPass();
            }

            BeginRenderPass();

            var clearValue = new ClearValue(new ClearColorValue(color.Red, color.Green, color.Blue, color.Alpha));
            var attachment = new ClearAttachment(ImageAspectFlags.ImageAspectColorBit, (uint)index, clearValue);
            var clearRect = FramebufferParams.GetClearRect(ClearScissor, layer, layerCount);

            Gd.Api.CmdClearAttachments(CommandBuffer, 1, &attachment, 1, &clearRect);
        }

        public unsafe void ClearRenderTargetDepthStencil(int layer, int layerCount, float depthValue, bool depthMask, int stencilValue, int stencilMask)
        {
            // TODO: Use stencilMask (fully)

            if (FramebufferParams == null || !FramebufferParams.HasDepthStencil)
            {
                return;
            }

            if (_renderPass == null)
            {
                CreateRenderPass();
            }

            BeginRenderPass();

            var clearValue = new ClearValue(null, new ClearDepthStencilValue(depthValue, (uint)stencilValue));
            var flags = depthMask ? ImageAspectFlags.ImageAspectDepthBit : 0;

            if (stencilMask != 0)
            {
                flags |= ImageAspectFlags.ImageAspectStencilBit;
            }

            var attachment = new ClearAttachment(flags, 0, clearValue);
            var clearRect = FramebufferParams.GetClearRect(ClearScissor, layer, layerCount);

            Gd.Api.CmdClearAttachments(CommandBuffer, 1, &attachment, 1, &clearRect);
        }

        public void CommandBufferBarrier()
        {
            // TODO: More specific barrier?
            Barrier();
        }

        public void CopyBuffer(BufferHandle source, BufferHandle destination, int srcOffset, int dstOffset, int size)
        {
            EndRenderPass();

            var src = Gd.BufferManager.GetBuffer(CommandBuffer, source, srcOffset, size, false);
            var dst = Gd.BufferManager.GetBuffer(CommandBuffer, destination, dstOffset, size, true);

            BufferHolder.Copy(Gd, Cbs, src, dst, srcOffset, dstOffset, size);
        }

        public void DirtyVertexBuffer(Auto<DisposableBuffer> buffer)
        {
            for (int i = 0; i < _vertexBuffers.Length; i++)
            {
                if (_vertexBuffers[i].BoundEquals(buffer))
                {
                    _vertexBuffersDirty |= 1UL << i;
                }
            }
        }

        public void DirtyIndexBuffer(Auto<DisposableBuffer> buffer)
        {
            if (_indexBuffer.BoundEquals(buffer))
            {
                _needsIndexBufferRebind = true;
            }
        }

        public void DispatchCompute(int groupsX, int groupsY, int groupsZ)
        {
            if (!_program.IsLinked)
            {
                return;
            }

            EndRenderPass();
            RecreatePipelineIfNeeded(PipelineBindPoint.Compute);

            Gd.Api.CmdDispatch(CommandBuffer, (uint)groupsX, (uint)groupsY, (uint)groupsZ);
        }

        public void Draw(int vertexCount, int instanceCount, int firstVertex, int firstInstance)
        {
            if (!_program.IsLinked)
            {
                return;
            }

            RecreatePipelineIfNeeded(PipelineBindPoint.Graphics);
            BeginRenderPass();
            DrawCount++;

            if (Gd.TopologyUnsupported(_topology))
            {
                // Temporarily bind a conversion pattern as an index buffer.
                _needsIndexBufferRebind = true;

                IndexBufferPattern pattern = _topology switch
                {
                    GAL.PrimitiveTopology.Quads => QuadsToTrisPattern,
                    GAL.PrimitiveTopology.TriangleFan => TriFanToTrisPattern,
                    _ => throw new NotSupportedException($"Unsupported topology: {_topology}")
                };

                BufferHandle handle = pattern.GetRepeatingBuffer(vertexCount, out int indexCount);
                var buffer = Gd.BufferManager.GetBuffer(CommandBuffer, handle, false);

                Gd.Api.CmdBindIndexBuffer(CommandBuffer, buffer.Get(Cbs, 0, indexCount * sizeof(int)).Value, 0, Silk.NET.Vulkan.IndexType.Uint32);

                BeginRenderPass(); // May have been interrupted to set buffer data.
                ResumeTransformFeedbackInternal();

                Gd.Api.CmdDrawIndexed(CommandBuffer, (uint)indexCount, (uint)instanceCount, 0, firstVertex, (uint)firstInstance);
            }
            else
            {
                ResumeTransformFeedbackInternal();

                Gd.Api.CmdDraw(CommandBuffer, (uint)vertexCount, (uint)instanceCount, (uint)firstVertex, (uint)firstInstance);
            }
        }

        private void UpdateIndexBufferPattern()
        {
            IndexBufferPattern pattern = null;

            if (Gd.TopologyUnsupported(_topology))
            {
                pattern = _topology switch
                {
                    GAL.PrimitiveTopology.Quads => QuadsToTrisPattern,
                    GAL.PrimitiveTopology.TriangleFan => TriFanToTrisPattern,
                    _ => throw new NotSupportedException($"Unsupported topology: {_topology}")
                };
            }

            if (_indexBufferPattern != pattern)
            {
                _indexBufferPattern = pattern;
                _needsIndexBufferRebind = true;
            }
        }

        public void DrawIndexed(int indexCount, int instanceCount, int firstIndex, int firstVertex, int firstInstance)
        {
            if (!_program.IsLinked)
            {
                return;
            }

            UpdateIndexBufferPattern();
            RecreatePipelineIfNeeded(PipelineBindPoint.Graphics);
            BeginRenderPass();
            DrawCount++;

            if (_indexBufferPattern != null)
            {
                // Convert the index buffer into a supported topology.
                IndexBufferPattern pattern = _indexBufferPattern;

                int convertedCount = pattern.GetConvertedCount(indexCount);

                if (_needsIndexBufferRebind)
                {
                    _indexBuffer.BindConvertedIndexBuffer(Gd, Cbs, firstIndex, indexCount, convertedCount, pattern);

                    _needsIndexBufferRebind = false;
                }

                BeginRenderPass(); // May have been interrupted to set buffer data.
                ResumeTransformFeedbackInternal();

                Gd.Api.CmdDrawIndexed(CommandBuffer, (uint)convertedCount, (uint)instanceCount, 0, firstVertex, (uint)firstInstance);
            }
            else
            {
                ResumeTransformFeedbackInternal();

                Gd.Api.CmdDrawIndexed(CommandBuffer, (uint)indexCount, (uint)instanceCount, (uint)firstIndex, firstVertex, (uint)firstInstance);
            }
        }

        public void DrawTexture(ITexture texture, ISampler sampler, Extents2DF srcRegion, Extents2DF dstRegion)
        {
            if (texture is TextureView srcTexture)
            {
                SupportBufferUpdater.Commit();

                var oldCullMode = _newState.CullMode;
                var oldStencilTestEnable = _newState.StencilTestEnable;
                var oldDepthTestEnable = _newState.DepthTestEnable;
                var oldDepthWriteEnable = _newState.DepthWriteEnable;
                var oldTopology = _newState.Topology;
                var oldViewports = DynamicState.Viewports;
                var oldViewportsCount = _newState.ViewportsCount;

                _newState.CullMode = CullModeFlags.CullModeNone;
                _newState.StencilTestEnable = false;
                _newState.DepthTestEnable = false;
                _newState.DepthWriteEnable = false;
                SignalStateChange();

                Gd.HelperShader.DrawTexture(
                    Gd,
                    this,
                    srcTexture,
                    sampler,
                    srcRegion,
                    dstRegion);

                _newState.CullMode = oldCullMode;
                _newState.StencilTestEnable = oldStencilTestEnable;
                _newState.DepthTestEnable = oldDepthTestEnable;
                _newState.DepthWriteEnable = oldDepthWriteEnable;
                _newState.Topology = oldTopology;

                DynamicState.Viewports = oldViewports;
                DynamicState.ViewportsCount = (int)oldViewportsCount;
                DynamicState.SetViewportsDirty();

                _newState.ViewportsCount = oldViewportsCount;
                SignalStateChange();
            }
        }

        public void EndTransformFeedback()
        {
            PauseTransformFeedbackInternal();
            _tfEnabled = false;
        }

        public bool IsCommandBufferActive(CommandBuffer cb)
        {
            return CommandBuffer.Handle == cb.Handle;
        }

        public void MultiDrawIndirectCount(BufferRange indirectBuffer, BufferRange parameterBuffer, int maxDrawCount, int stride)
        {
            if (!Gd.Capabilities.SupportsIndirectParameters)
            {
                throw new NotSupportedException();
            }

            if (_program.LinkStatus != ProgramLinkStatus.Success)
            {
                return;
            }

            RecreatePipelineIfNeeded(PipelineBindPoint.Graphics);
            BeginRenderPass();
            ResumeTransformFeedbackInternal();
            DrawCount++;

            var buffer = Gd.BufferManager
                .GetBuffer(CommandBuffer, indirectBuffer.Handle, indirectBuffer.Offset, indirectBuffer.Size, true)
                .Get(Cbs, indirectBuffer.Offset, indirectBuffer.Size).Value;

            var countBuffer = Gd.BufferManager
                .GetBuffer(CommandBuffer, parameterBuffer.Handle, parameterBuffer.Offset, parameterBuffer.Size, true)
                .Get(Cbs, parameterBuffer.Offset, parameterBuffer.Size).Value;

            Gd.DrawIndirectCountApi.CmdDrawIndirectCount(
                CommandBuffer,
                buffer,
                (ulong)indirectBuffer.Offset,
                countBuffer,
                (ulong)parameterBuffer.Offset,
                (uint)maxDrawCount,
                (uint)stride);
        }

        public void MultiDrawIndexedIndirectCount(BufferRange indirectBuffer, BufferRange parameterBuffer, int maxDrawCount, int stride)
        {
            if (!Gd.Capabilities.SupportsIndirectParameters)
            {
                throw new NotSupportedException();
            }

            if (_program.LinkStatus != ProgramLinkStatus.Success)
            {
                return;
            }

            RecreatePipelineIfNeeded(PipelineBindPoint.Graphics);
            BeginRenderPass();
            ResumeTransformFeedbackInternal();
            DrawCount++;

            var buffer = Gd.BufferManager
                .GetBuffer(CommandBuffer, indirectBuffer.Handle, parameterBuffer.Offset, parameterBuffer.Size, true)
                .Get(Cbs, indirectBuffer.Offset, indirectBuffer.Size).Value;

            var countBuffer = Gd.BufferManager
                .GetBuffer(CommandBuffer, parameterBuffer.Handle, parameterBuffer.Offset, parameterBuffer.Size, true)
                .Get(Cbs, parameterBuffer.Offset, parameterBuffer.Size).Value;

            Gd.DrawIndirectCountApi.CmdDrawIndexedIndirectCount(
                CommandBuffer,
                buffer,
                (ulong)indirectBuffer.Offset,
                countBuffer,
                (ulong)parameterBuffer.Offset,
                (uint)maxDrawCount,
                (uint)stride);
        }

        public void SetAlphaTest(bool enable, float reference, GAL.CompareOp op)
        {
            // This is currently handled using shader specialization, as Vulkan does not support alpha test.
            // In the future, we may want to use this to write the reference value into the support buffer,
            // to avoid creating one version of the shader per reference value used.
        }

        public void SetBlendState(int index, BlendDescriptor blend)
        {
            ref var vkBlend = ref _newState.Internal.ColorBlendAttachmentState[index];

            if (blend.Enable)
            {
                vkBlend.BlendEnable = blend.Enable;
                vkBlend.SrcColorBlendFactor = blend.ColorSrcFactor.Convert();
                vkBlend.DstColorBlendFactor = blend.ColorDstFactor.Convert();
                vkBlend.ColorBlendOp = blend.ColorOp.Convert();
                vkBlend.SrcAlphaBlendFactor = blend.AlphaSrcFactor.Convert();
                vkBlend.DstAlphaBlendFactor = blend.AlphaDstFactor.Convert();
                vkBlend.AlphaBlendOp = blend.AlphaOp.Convert();
            }
            else
            {
                vkBlend = new PipelineColorBlendAttachmentState(
                    colorWriteMask: vkBlend.ColorWriteMask);
            }

            if (vkBlend.ColorWriteMask == 0)
            {
                _storedBlend[index] = vkBlend;

                vkBlend = new PipelineColorBlendAttachmentState();
            }

            _newState.BlendConstantR = blend.BlendConstant.Red;
            _newState.BlendConstantG = blend.BlendConstant.Green;
            _newState.BlendConstantB = blend.BlendConstant.Blue;
            _newState.BlendConstantA = blend.BlendConstant.Alpha;

            SignalStateChange();
        }

        public void SetDepthBias(PolygonModeMask enables, float factor, float units, float clamp)
        {
            DynamicState.SetDepthBias(factor, units, clamp);

            _newState.DepthBiasEnable = enables != 0;
            SignalStateChange();
        }

        public void SetDepthClamp(bool clamp)
        {
            _newState.DepthClampEnable = clamp;
            SignalStateChange();
        }

        public void SetDepthMode(DepthMode mode)
        {
            // Currently this is emulated on the shader, because Vulkan had no support for changing the depth mode.
            // In the future, we may want to use the VK_EXT_depth_clip_control extension to change it here.
        }

        public void SetDepthTest(DepthTestDescriptor depthTest)
        {
            _newState.DepthTestEnable = depthTest.TestEnable;
            _newState.DepthWriteEnable = depthTest.WriteEnable;
            _newState.DepthCompareOp = depthTest.Func.Convert();
            SignalStateChange();
        }

        public void SetFaceCulling(bool enable, Face face)
        {
            _newState.CullMode = enable ? face.Convert() : CullModeFlags.CullModeNone;
            SignalStateChange();
        }

        public void SetFrontFace(GAL.FrontFace frontFace)
        {
            _newState.FrontFace = frontFace.Convert();
            SignalStateChange();
        }

        public void SetImage(int binding, ITexture image, GAL.Format imageFormat)
        {
            _descriptorSetUpdater.SetImage(binding, image, imageFormat);
        }

        public void SetIndexBuffer(BufferRange buffer, GAL.IndexType type)
        {
            if (buffer.Handle != BufferHandle.Null)
            {
                _indexBuffer = new IndexBufferState(buffer.Handle, buffer.Offset, buffer.Size, type.Convert());
            }
            else
            {
                _indexBuffer = IndexBufferState.Null;
            }

            _needsIndexBufferRebind = true;
        }

        public void SetLineParameters(float width, bool smooth)
        {
            _newState.LineWidth = width;
            SignalStateChange();
        }

        public void SetLogicOpState(bool enable, LogicalOp op)
        {
            _newState.LogicOpEnable = enable;
            _newState.LogicOp = op.Convert();
            SignalStateChange();
        }

        public void SetMultisampleState(MultisampleDescriptor multisample)
        {
            _newState.AlphaToCoverageEnable = multisample.AlphaToCoverageEnable;
            _newState.AlphaToOneEnable = multisample.AlphaToOneEnable;
            SignalStateChange();
        }

        public void SetOrigin(Origin origin)
        {
            // TODO.
        }

        public unsafe void SetPatchParameters(int vertices, ReadOnlySpan<float> defaultOuterLevel, ReadOnlySpan<float> defaultInnerLevel)
        {
            _newState.PatchControlPoints = (uint)vertices;
            SignalStateChange();

            // TODO: Default levels (likely needs emulation on shaders?)
        }

        public void SetPointParameters(float size, bool isProgramPointSize, bool enablePointSprite, Origin origin)
        {
            // TODO.
        }

        public void SetPolygonMode(GAL.PolygonMode frontMode, GAL.PolygonMode backMode)
        {
            // TODO.
        }

        public void SetPrimitiveRestart(bool enable, int index)
        {
            _newState.PrimitiveRestartEnable = enable;
            // TODO: What to do about the index?
            SignalStateChange();
        }

        public void SetPrimitiveTopology(GAL.PrimitiveTopology topology)
        {
            _topology = topology;

            var vkTopology = Gd.TopologyRemap(topology).Convert();

            _newState.Topology = vkTopology;

            SignalStateChange();
        }

        public void SetProgram(IProgram program)
        {
            var internalProgram = (ShaderCollection)program;
            var stages = internalProgram.GetInfos();

            _program = internalProgram;

            _descriptorSetUpdater.SetProgram(internalProgram);

            _newState.PipelineLayout = internalProgram.PipelineLayout;
            _newState.StagesCount = (uint)stages.Length;

            stages.CopyTo(_newState.Stages.AsSpan().Slice(0, stages.Length));

            SignalStateChange();
        }

        protected virtual void SignalAttachmentChange()
        {
        }

        public void SetRasterizerDiscard(bool discard)
        {
            _newState.RasterizerDiscardEnable = discard;
            SignalStateChange();
        }

        public void SetRenderTargetColorMasks(ReadOnlySpan<uint> componentMask)
        {
            int count = Math.Min(Constants.MaxRenderTargets, componentMask.Length);
            int writtenAttachments = 0;

            for (int i = 0; i < count; i++)
            {
                ref var vkBlend = ref _newState.Internal.ColorBlendAttachmentState[i];
                var newMask = (ColorComponentFlags)componentMask[i];

                // When color write mask is 0, remove all blend state to help the pipeline cache.
                // Restore it when the mask becomes non-zero.
                if (vkBlend.ColorWriteMask != newMask)
                {
                    if (newMask == 0)
                    {
                        _storedBlend[i] = vkBlend;

                        vkBlend = new PipelineColorBlendAttachmentState();
                    }
                    else if (vkBlend.ColorWriteMask == 0)
                    {
                        vkBlend = _storedBlend[i];
                    }
                }

                vkBlend.ColorWriteMask = newMask;

                if (componentMask[i] != 0)
                {
                    writtenAttachments++;
                }
            }

            SignalStateChange();

            if (writtenAttachments != _writtenAttachmentCount)
            {
                SignalAttachmentChange();
                _writtenAttachmentCount = writtenAttachments;
            }
        }

        public void SetRenderTargets(ITexture[] colors, ITexture depthStencil)
        {
            FramebufferParams?.UpdateModifications();
            CreateFramebuffer(colors, depthStencil);
            CreateRenderPass();
            SignalStateChange();
            SignalAttachmentChange();
        }

        public void SetRenderTargetScale(float scale)
        {
            _renderScale[0].X = scale;
            SupportBufferUpdater.UpdateRenderScale(_renderScale, 0, 1); // Just the first element.
        }

        public void SetScissors(ReadOnlySpan<Rectangle<int>> regions)
        {
            int maxScissors = Gd.Capabilities.SupportsMultiView ? Constants.MaxViewports : 1;
            int count = Math.Min(maxScissors, regions.Length);
            if (count > 0)
            {
                ClearScissor = regions[0];
            }

            for (int i = 0; i < count; i++)
            {
                var region = regions[i];
                var offset = new Offset2D(region.X, region.Y);
                var extent = new Extent2D((uint)region.Width, (uint)region.Height);

                DynamicState.SetScissor(i, new Rect2D(offset, extent));
            }

            DynamicState.ScissorsCount = count;

            _newState.ScissorsCount = (uint)count;
            SignalStateChange();
        }

        public void SetStencilTest(StencilTestDescriptor stencilTest)
        {
            DynamicState.SetStencilMasks(
                (uint)stencilTest.BackFuncMask,
                (uint)stencilTest.BackMask,
                (uint)stencilTest.BackFuncRef,
                (uint)stencilTest.FrontFuncMask,
                (uint)stencilTest.FrontMask,
                (uint)stencilTest.FrontFuncRef);

            _newState.StencilTestEnable = stencilTest.TestEnable;
            _newState.StencilBackFailOp = stencilTest.BackSFail.Convert();
            _newState.StencilBackPassOp = stencilTest.BackDpPass.Convert();
            _newState.StencilBackDepthFailOp = stencilTest.BackDpFail.Convert();
            _newState.StencilBackCompareOp = stencilTest.BackFunc.Convert();
            _newState.StencilFrontFailOp = stencilTest.FrontSFail.Convert();
            _newState.StencilFrontPassOp = stencilTest.FrontDpPass.Convert();
            _newState.StencilFrontDepthFailOp = stencilTest.FrontDpFail.Convert();
            _newState.StencilFrontCompareOp = stencilTest.FrontFunc.Convert();
            SignalStateChange();
        }

        public void SetStorageBuffers(int first, ReadOnlySpan<BufferRange> buffers)
        {
            _descriptorSetUpdater.SetStorageBuffers(CommandBuffer, first, buffers);
        }

        public void SetStorageBuffers(int first, ReadOnlySpan<Auto<DisposableBuffer>> buffers)
        {
            _descriptorSetUpdater.SetStorageBuffers(CommandBuffer, first, buffers);
        }

        public void SetTextureAndSampler(ShaderStage stage, int binding, ITexture texture, ISampler sampler)
        {
            _descriptorSetUpdater.SetTextureAndSampler(Cbs, stage, binding, texture, sampler);
        }

        public void SetTransformFeedbackBuffers(ReadOnlySpan<BufferRange> buffers)
        {
            PauseTransformFeedbackInternal();

            int count = Math.Min(Constants.MaxTransformFeedbackBuffers, buffers.Length);

            for (int i = 0; i < count; i++)
            {
                var range = buffers[i];

                _transformFeedbackBuffers[i].Dispose();

                if (range.Handle != BufferHandle.Null)
                {
                    _transformFeedbackBuffers[i] = 
                        new BufferState(Gd.BufferManager.GetBuffer(CommandBuffer, range.Handle, range.Offset, range.Size, true), range.Offset, range.Size);
                    _transformFeedbackBuffers[i].BindTransformFeedbackBuffer(Gd, Cbs, (uint)i);
                }
                else
                {
                    _transformFeedbackBuffers[i] = BufferState.Null;
                }
            }
        }

        public void SetUniformBuffers(int first, ReadOnlySpan<BufferRange> buffers)
        {
            _descriptorSetUpdater.SetUniformBuffers(CommandBuffer, first, buffers);
        }

        public void SetUserClipDistance(int index, bool enableClip)
        {
            // TODO.
        }

        public void SetVertexAttribs(ReadOnlySpan<VertexAttribDescriptor> vertexAttribs)
        {
            var formatCapabilities = Gd.FormatCapabilities;

            Span<int> newVbScalarSizes = stackalloc int[Constants.MaxVertexBuffers];

            int count = Math.Min(Constants.MaxVertexAttributes, vertexAttribs.Length);
            uint dirtyVbSizes = 0;

            for (int i = 0; i < count; i++)
            {
                var attribute = vertexAttribs[i];
                var rawIndex = attribute.BufferIndex;
                var bufferIndex = attribute.IsZero ? 0 : rawIndex + 1;

                if (!attribute.IsZero)
                {
                    newVbScalarSizes[rawIndex] = Math.Max(newVbScalarSizes[rawIndex], attribute.Format.GetScalarSize());
                    dirtyVbSizes |= 1u << rawIndex;
                }

                _newState.Internal.VertexAttributeDescriptions[i] = new VertexInputAttributeDescription(
                    (uint)i,
                    (uint)bufferIndex,
                    formatCapabilities.ConvertToVertexVkFormat(attribute.Format),
                    (uint)attribute.Offset);
            }

            while (dirtyVbSizes != 0)
            {
                int dirtyBit = BitOperations.TrailingZeroCount(dirtyVbSizes);

                ref var buffer = ref _vertexBuffers[dirtyBit + 1];

                if (buffer.AttributeScalarAlignment != newVbScalarSizes[dirtyBit])
                {
                    _vertexBuffersDirty |= 1UL << (dirtyBit + 1);
                    buffer.AttributeScalarAlignment = newVbScalarSizes[dirtyBit];
                }

                dirtyVbSizes &= ~(1u << dirtyBit);
            }

            _newState.VertexAttributeDescriptionsCount = (uint)count;
            SignalStateChange();
        }

        public void SetVertexBuffers(ReadOnlySpan<VertexBufferDescriptor> vertexBuffers)
        {
            int count = Math.Min(Constants.MaxVertexBuffers, vertexBuffers.Length);

            _newState.Internal.VertexBindingDescriptions[0] = new VertexInputBindingDescription(0, 0, VertexInputRate.Vertex);

            int validCount = 1;

            for (int i = 0; i < count; i++)
            {
                var vertexBuffer = vertexBuffers[i];

                // TODO: Support divisor > 1
                var inputRate = vertexBuffer.Divisor != 0 ? VertexInputRate.Instance : VertexInputRate.Vertex;

                if (vertexBuffer.Buffer.Handle != BufferHandle.Null)
                {
                    var vb = Gd.BufferManager.GetBuffer(CommandBuffer, vertexBuffer.Buffer.Handle, false);
                    if (vb != null)
                    {
                        int binding = i + 1;
                        int descriptorIndex = validCount++;

                        _newState.Internal.VertexBindingDescriptions[descriptorIndex] = new VertexInputBindingDescription(
                            (uint)binding,
                            (uint)vertexBuffer.Stride,
                            inputRate);

                        int vbSize = vertexBuffer.Buffer.Size;

                        if (Gd.Vendor == Vendor.Amd && vertexBuffer.Stride > 0)
                        {
                            // AMD has a bug where if offset + stride * count is greater than
                            // the size, then the last attribute will have the wrong value.
                            // As a workaround, simply use the full buffer size.
                            int remainder = vbSize % vertexBuffer.Stride;
                            if (remainder != 0)
                            {
                                vbSize += vertexBuffer.Stride - remainder;
                            }
                        }

                        ref var buffer = ref _vertexBuffers[binding];
                        int oldScalarAlign = buffer.AttributeScalarAlignment;

                        buffer.Dispose();

                        if ((vertexBuffer.Stride % FormatExtensions.MaxBufferFormatScalarSize) == 0)
                        {
                            buffer = new VertexBufferState(
                                vb,
                                descriptorIndex,
                                vertexBuffer.Buffer.Offset,
                                vbSize,
                                vertexBuffer.Stride);

                            buffer.BindVertexBuffer(Gd, Cbs, (uint)binding, ref _newState);
                        }
                        else
                        {
                            // May need to be rewritten. Bind this buffer before draw.

                            buffer = new VertexBufferState(
                                vertexBuffer.Buffer.Handle,
                                descriptorIndex,
                                vertexBuffer.Buffer.Offset,
                                vbSize,
                                vertexBuffer.Stride);

                            _vertexBuffersDirty |= 1UL << binding;
                        }

                        buffer.AttributeScalarAlignment = oldScalarAlign;
                    }
                }
            }

            _newState.VertexBindingDescriptionsCount = (uint)validCount;
            SignalStateChange();
        }

        public void SetViewports(ReadOnlySpan<GAL.Viewport> viewports, bool disableTransform)
        {
            int maxViewports = Gd.Capabilities.SupportsMultiView ? Constants.MaxViewports : 1;
            int count = Math.Min(maxViewports, viewports.Length);

            static float Clamp(float value)
            {
                return Math.Clamp(value, 0f, 1f);
            }

            for (int i = 0; i < count; i++)
            {
                var viewport = viewports[i];

                DynamicState.SetViewport(i, new Silk.NET.Vulkan.Viewport(
                    viewport.Region.X,
                    viewport.Region.Y,
                    viewport.Region.Width == 0f ? 1f : viewport.Region.Width,
                    viewport.Region.Height == 0f ? 1f : viewport.Region.Height,
                    Clamp(viewport.DepthNear),
                    Clamp(viewport.DepthFar)));
            }

            DynamicState.ViewportsCount = count;

            float disableTransformF = disableTransform ? 1.0f : 0.0f;
            if (SupportBufferUpdater.Data.ViewportInverse.W != disableTransformF || disableTransform)
            {
                float scale = _renderScale[0].X;
                SupportBufferUpdater.UpdateViewportInverse(new Vector4<float>
                {
                    X = scale * 2f / viewports[0].Region.Width,
                    Y = scale * 2f / viewports[0].Region.Height,
                    Z = 1,
                    W = disableTransformF
                });
            }

            _newState.ViewportsCount = (uint)count;
            SignalStateChange();
        }

        public unsafe void TextureBarrier()
        {
            MemoryBarrier memoryBarrier = new MemoryBarrier()
            {
                SType = StructureType.MemoryBarrier,
                SrcAccessMask = AccessFlags.AccessMemoryReadBit | AccessFlags.AccessMemoryWriteBit,
                DstAccessMask = AccessFlags.AccessMemoryReadBit | AccessFlags.AccessMemoryWriteBit
            };

            Gd.Api.CmdPipelineBarrier(
                CommandBuffer,
                PipelineStageFlags.PipelineStageFragmentShaderBit,
                PipelineStageFlags.PipelineStageFragmentShaderBit,
                0,
                1,
                memoryBarrier,
                0,
                null,
                0,
                null);
        }

        public void TextureBarrierTiled()
        {
            TextureBarrier();
        }

        public void UpdateRenderScale(ReadOnlySpan<float> scales, int totalCount, int fragmentCount)
        {
            bool changed = false;

            for (int index = 0; index < totalCount; index++)
            {
                if (_renderScale[1 + index].X != scales[index])
                {
                    _renderScale[1 + index].X = scales[index];
                    changed = true;
                }
            }

            // Only update fragment count if there are scales after it for the vertex stage.
            if (fragmentCount != totalCount && fragmentCount != _fragmentScaleCount)
            {
                _fragmentScaleCount = fragmentCount;
                SupportBufferUpdater.UpdateFragmentRenderScaleCount(_fragmentScaleCount);
            }

            if (changed)
            {
                SupportBufferUpdater.UpdateRenderScale(_renderScale, 0, 1 + totalCount);
            }
        }

        protected void SignalCommandBufferChange()
        {
            _needsIndexBufferRebind = true;
            _needsTransformFeedbackBuffersRebind = true;
            _vertexBuffersDirty = ulong.MaxValue >> (64 - _vertexBuffers.Length);

            _descriptorSetUpdater.SignalCommandBufferChange();
            DynamicState.ForceAllDirty();
            _currentPipelineHandle = 0;
        }

        private void CreateFramebuffer(ITexture[] colors, ITexture depthStencil)
        {
            FramebufferParams = new FramebufferParams(Device, colors, depthStencil);
            UpdatePipelineAttachmentFormats();
            _newState.SamplesCount = FramebufferParams.AttachmentSamples.Length != 0 ? FramebufferParams.AttachmentSamples[0] : 1;
        }

        protected void UpdatePipelineAttachmentFormats()
        {
            var dstAttachmentFormats = _newState.Internal.AttachmentFormats.AsSpan();
            FramebufferParams.AttachmentFormats.CopyTo(dstAttachmentFormats);

            int maxAttachmentIndex = FramebufferParams.MaxColorAttachmentIndex + (FramebufferParams.HasDepthStencil ? 1 : 0);
            for (int i = FramebufferParams.AttachmentFormats.Length; i <= maxAttachmentIndex; i++)
            {
                dstAttachmentFormats[i] = 0;
            }

            _newState.ColorBlendAttachmentStateCount = (uint)(FramebufferParams.MaxColorAttachmentIndex + 1);
            _newState.HasDepthStencil = FramebufferParams.HasDepthStencil;
        }

        protected unsafe void CreateRenderPass()
        {
            const int MaxAttachments = Constants.MaxRenderTargets + 1;

            AttachmentDescription[] attachmentDescs = null;

            var subpass = new SubpassDescription()
            {
                PipelineBindPoint = PipelineBindPoint.Graphics
            };

            AttachmentReference* attachmentReferences = stackalloc AttachmentReference[MaxAttachments];

            var hasFramebuffer = FramebufferParams != null;

            if (hasFramebuffer && FramebufferParams.AttachmentsCount != 0)
            {
                attachmentDescs = new AttachmentDescription[FramebufferParams.AttachmentsCount];

                for (int i = 0; i < FramebufferParams.AttachmentsCount; i++)
                {
                    int bindIndex = FramebufferParams.AttachmentIndices[i];

                    attachmentDescs[i] = new AttachmentDescription(
                        0,
                        FramebufferParams.AttachmentFormats[i],
                        TextureStorage.ConvertToSampleCountFlags(FramebufferParams.AttachmentSamples[i]),
                        AttachmentLoadOp.Load,
                        AttachmentStoreOp.Store,
                        AttachmentLoadOp.Load,
                        AttachmentStoreOp.Store,
                        ImageLayout.General,
                        ImageLayout.General);
                }

                int colorAttachmentsCount = FramebufferParams.ColorAttachmentsCount;

                if (colorAttachmentsCount > MaxAttachments - 1)
                {
                    colorAttachmentsCount = MaxAttachments - 1;
                }

                if (colorAttachmentsCount != 0)
                {
                    int maxAttachmentIndex = FramebufferParams.MaxColorAttachmentIndex;
                    subpass.ColorAttachmentCount = (uint)maxAttachmentIndex + 1;
                    subpass.PColorAttachments = &attachmentReferences[0];

                    // Fill with VK_ATTACHMENT_UNUSED to cover any gaps.
                    for (int i = 0; i <= maxAttachmentIndex; i++)
                    {
                        subpass.PColorAttachments[i] = new AttachmentReference(Vk.AttachmentUnused, ImageLayout.Undefined);
                    }

                    for (int i = 0; i < colorAttachmentsCount; i++)
                    {
                        int bindIndex = FramebufferParams.AttachmentIndices[i];

                        subpass.PColorAttachments[bindIndex] = new AttachmentReference((uint)i, ImageLayout.General);
                    }
                }

                if (FramebufferParams.HasDepthStencil)
                {
                    uint dsIndex = (uint)FramebufferParams.AttachmentsCount - 1;

                    subpass.PDepthStencilAttachment = &attachmentReferences[MaxAttachments - 1];
                    *subpass.PDepthStencilAttachment = new AttachmentReference(dsIndex, ImageLayout.General);
                }
            }

            var subpassDependency = new SubpassDependency(
                0,
                0,
                PipelineStageFlags.PipelineStageAllGraphicsBit,
                PipelineStageFlags.PipelineStageAllGraphicsBit,
                AccessFlags.AccessMemoryReadBit | AccessFlags.AccessMemoryWriteBit | AccessFlags.AccessColorAttachmentWriteBit,
                AccessFlags.AccessMemoryReadBit | AccessFlags.AccessMemoryWriteBit | AccessFlags.AccessShaderReadBit,
                0);

            fixed (AttachmentDescription* pAttachmentDescs = attachmentDescs)
            {
                var renderPassCreateInfo = new RenderPassCreateInfo()
                {
                    SType = StructureType.RenderPassCreateInfo,
                    PAttachments = pAttachmentDescs,
                    AttachmentCount = attachmentDescs != null ? (uint)attachmentDescs.Length : 0,
                    PSubpasses = &subpass,
                    SubpassCount = 1,
                    PDependencies = &subpassDependency,
                    DependencyCount = 1
                };

                Gd.Api.CreateRenderPass(Device, renderPassCreateInfo, null, out var renderPass).ThrowOnError();

                _renderPass?.Dispose();
                _renderPass = new Auto<DisposableRenderPass>(new DisposableRenderPass(Gd.Api, Device, renderPass));
            }

            EndRenderPass();

            _framebuffer?.Dispose();
            _framebuffer = hasFramebuffer ? FramebufferParams.Create(Gd.Api, Cbs, _renderPass) : null;
        }

        protected void SignalStateChange()
        {
            _stateDirty = true;
        }

        private void RecreatePipelineIfNeeded(PipelineBindPoint pbp)
        {
            DynamicState.ReplayIfDirty(Gd.Api, CommandBuffer);

            // Commit changes to the support buffer before drawing.
            SupportBufferUpdater.Commit();

            if (_needsIndexBufferRebind && _indexBufferPattern == null)
            {
                _indexBuffer.BindIndexBuffer(Gd, Cbs);
                _needsIndexBufferRebind = false;
            }

            if (_needsTransformFeedbackBuffersRebind)
            {
                PauseTransformFeedbackInternal();

                for (int i = 0; i < Constants.MaxTransformFeedbackBuffers; i++)
                {
                    _transformFeedbackBuffers[i].BindTransformFeedbackBuffer(Gd, Cbs, (uint)i);
                }

                _needsTransformFeedbackBuffersRebind = false;
            }

            if (_vertexBuffersDirty != 0)
            {
                while (_vertexBuffersDirty != 0)
                {
                    int i = BitOperations.TrailingZeroCount(_vertexBuffersDirty);

                    _vertexBuffers[i].BindVertexBuffer(Gd, Cbs, (uint)i, ref _newState);

                    _vertexBuffersDirty &= ~(1u << i);
                }
            }

            if (_stateDirty || Pbp != pbp)
            {
                CreatePipeline(pbp);
                _stateDirty = false;
                Pbp = pbp;
            }

            _descriptorSetUpdater.UpdateAndBindDescriptorSets(Cbs, pbp);
        }

        private void CreatePipeline(PipelineBindPoint pbp)
        {
            // We can only create a pipeline if the have the shader stages set.
            if (_newState.Stages != null)
            {
                if (pbp == PipelineBindPoint.Graphics && _renderPass == null)
                {
                    CreateRenderPass();
                }

                var pipeline = pbp == PipelineBindPoint.Compute
                    ? _newState.CreateComputePipeline(Gd, Device, _program, PipelineCache)
                    : _newState.CreateGraphicsPipeline(Gd, Device, _program, PipelineCache, _renderPass.Get(Cbs).Value);

                ulong pipelineHandle = pipeline.GetUnsafe().Value.Handle;

                if (_currentPipelineHandle != pipelineHandle)
                {
                    _currentPipelineHandle = pipelineHandle;
                    Pipeline = pipeline;

                    PauseTransformFeedbackInternal();
                    Gd.Api.CmdBindPipeline(CommandBuffer, pbp, Pipeline.Get(Cbs).Value);
                }
            }
        }

        private unsafe void BeginRenderPass()
        {
            if (!_renderPassActive)
            {
                var renderArea = new Rect2D(null, new Extent2D(FramebufferParams.Width, FramebufferParams.Height));
                var clearValue = new ClearValue();

                var renderPassBeginInfo = new RenderPassBeginInfo()
                {
                    SType = StructureType.RenderPassBeginInfo,
                    RenderPass = _renderPass.Get(Cbs).Value,
                    Framebuffer = _framebuffer.Get(Cbs).Value,
                    RenderArea = renderArea,
                    PClearValues = &clearValue,
                    ClearValueCount = 1
                };

                Gd.Api.CmdBeginRenderPass(CommandBuffer, renderPassBeginInfo, SubpassContents.Inline);
                _renderPassActive = true;
            }
        }

        public void EndRenderPass()
        {
            if (_renderPassActive)
            {
                PauseTransformFeedbackInternal();
                Gd.Api.CmdEndRenderPass(CommandBuffer);
                SignalRenderPassEnd();
                _renderPassActive = false;
            }
        }

        protected virtual void SignalRenderPassEnd()
        {
        }

        private void PauseTransformFeedbackInternal()
        {
            if (_tfEnabled && _tfActive)
            {
                EndTransformFeedbackInternal();
                _tfActive = false;
            }
        }

        private void ResumeTransformFeedbackInternal()
        {
            if (_tfEnabled && !_tfActive)
            {
                BeginTransformFeedbackInternal();
                _tfActive = true;
            }
        }

        private unsafe void BeginTransformFeedbackInternal()
        {
            Gd.TransformFeedbackApi.CmdBeginTransformFeedback(CommandBuffer, 0, 0, null, null);
        }

        private unsafe void EndTransformFeedbackInternal()
        {
            Gd.TransformFeedbackApi.CmdEndTransformFeedback(CommandBuffer, 0, 0, null, null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _renderPass?.Dispose();
                _framebuffer?.Dispose();
                _newState.Dispose();
                _descriptorSetUpdater.Dispose();

                for (int i = 0; i < _vertexBuffers.Length; i++)
                {
                    _vertexBuffers[i].Dispose();
                }

                for (int i = 0; i < _transformFeedbackBuffers.Length; i++)
                {
                    _transformFeedbackBuffers[i].Dispose();
                }

                Pipeline?.Dispose();

                unsafe
                {
                    Gd.Api.DestroyPipelineCache(Device, PipelineCache, null);
                }

                SupportBufferUpdater.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}

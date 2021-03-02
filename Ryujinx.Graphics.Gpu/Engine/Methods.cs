using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Gpu.Shader;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Texture;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine
{
    using Texture = Image.Texture;

    /// <summary>
    /// GPU method implementations.
    /// </summary>
    partial class Methods
    {
        private readonly GpuContext _context;
        private readonly ShaderProgramInfo[] _currentProgramInfo;

        /// <summary>
        /// In-memory shader cache.
        /// </summary>
        public ShaderCache ShaderCache { get; }

        /// <summary>
        /// GPU buffer manager.
        /// </summary>
        public BufferManager BufferManager { get; }

        /// <summary>
        /// GPU texture manager.
        /// </summary>
        public TextureManager TextureManager { get; }

        private bool _isAnyVbInstanced;
        private bool _vsUsesInstanceId;

        private bool _forceShaderUpdate;

        private bool _prevTfEnable;

        /// <summary>
        /// Creates a new instance of the GPU methods class.
        /// </summary>
        /// <param name="context">GPU context</param>
        public Methods(GpuContext context)
        {
            _context = context;

            ShaderCache = new ShaderCache(_context);

            _currentProgramInfo = new ShaderProgramInfo[Constants.ShaderStages];

            BufferManager  = new BufferManager(context);
            TextureManager = new TextureManager(context);

            context.MemoryManager.MemoryUnmapped += _counterCache.MemoryUnmappedHandler;
            context.MemoryManager.MemoryUnmapped += TextureManager.MemoryUnmappedHandler;
            context.MemoryManager.MemoryUnmapped += BufferManager.MemoryUnmappedHandler;
        }

        /// <summary>
        /// Register callback for GPU method calls that triggers an action on the GPU.
        /// </summary>
        /// <param name="state">GPU state where the triggers will be registered</param>
        public void RegisterCallbacks(GpuState state)
        {
            state.RegisterCallback(MethodOffset.LaunchDma,      LaunchDma);
            state.RegisterCallback(MethodOffset.LoadInlineData, LoadInlineData);

            state.RegisterCallback(MethodOffset.Dispatch, Dispatch);

            state.RegisterCallback(MethodOffset.SyncpointAction, IncrementSyncpoint);

            state.RegisterCallback(MethodOffset.CopyBuffer,  CopyBuffer);
            state.RegisterCallback(MethodOffset.CopyTexture, CopyTexture);

            state.RegisterCallback(MethodOffset.TextureBarrier,      TextureBarrier);
            state.RegisterCallback(MethodOffset.TextureBarrierTiled, TextureBarrierTiled);

            state.RegisterCallback(MethodOffset.VbElementU8,  VbElementU8);
            state.RegisterCallback(MethodOffset.VbElementU16, VbElementU16);
            state.RegisterCallback(MethodOffset.VbElementU32, VbElementU32);

            state.RegisterCallback(MethodOffset.ResetCounter, ResetCounter);

            state.RegisterCallback(MethodOffset.DrawEnd,                      DrawEnd);
            state.RegisterCallback(MethodOffset.DrawBegin,                    DrawBegin);
            state.RegisterCallback(MethodOffset.DrawIndexedSmall,             DrawIndexedSmall);
            state.RegisterCallback(MethodOffset.DrawIndexedSmall2,            DrawIndexedSmall2);
            state.RegisterCallback(MethodOffset.DrawIndexedSmallIncInstance,  DrawIndexedSmallIncInstance);
            state.RegisterCallback(MethodOffset.DrawIndexedSmallIncInstance2, DrawIndexedSmallIncInstance2);

            state.RegisterCallback(MethodOffset.IndexBufferCount, SetIndexBufferCount);

            state.RegisterCallback(MethodOffset.Clear, Clear);

            state.RegisterCallback(MethodOffset.Report, Report);

            state.RegisterCallback(MethodOffset.FirmwareCall4, FirmwareCall4);

            state.RegisterCallback(MethodOffset.UniformBufferUpdateData, 16, UniformBufferUpdate);

            state.RegisterCallback(MethodOffset.UniformBufferBindVertex,         UniformBufferBindVertex);
            state.RegisterCallback(MethodOffset.UniformBufferBindTessControl,    UniformBufferBindTessControl);
            state.RegisterCallback(MethodOffset.UniformBufferBindTessEvaluation, UniformBufferBindTessEvaluation);
            state.RegisterCallback(MethodOffset.UniformBufferBindGeometry,       UniformBufferBindGeometry);
            state.RegisterCallback(MethodOffset.UniformBufferBindFragment,       UniformBufferBindFragment);
        }

        /// <summary>
        /// Updates host state based on the current guest GPU state.
        /// </summary>
        /// <param name="state">Guest GPU state</param>
        /// <param name="firstIndex">Index of the first index buffer element used on the draw</param>
        /// <param name="indexCount">Number of index buffer elements used on the draw</param>
        private void UpdateState(GpuState state, int firstIndex, int indexCount)
        {
            bool tfEnable = state.Get<Boolean32>(MethodOffset.TfEnable);

            if (!tfEnable && _prevTfEnable)
            {
                _context.Renderer.Pipeline.EndTransformFeedback();
                _prevTfEnable = false;
            }

            // Shaders must be the first one to be updated if modified, because
            // some of the other state depends on information from the currently
            // bound shaders.
            if (state.QueryModified(MethodOffset.ShaderBaseAddress, MethodOffset.ShaderState) || _forceShaderUpdate)
            {
                _forceShaderUpdate = false;

                UpdateShaderState(state);
            }

            if (state.QueryModified(MethodOffset.TfBufferState))
            {
                UpdateTfBufferState(state);
            }

            if (state.QueryModified(MethodOffset.ClipDistanceEnable))
            {
                UpdateUserClipState(state);
            }

            if (state.QueryModified(MethodOffset.RasterizeEnable))
            {
                UpdateRasterizerState(state);
            }

            if (state.QueryModified(MethodOffset.RtColorState,
                                    MethodOffset.RtDepthStencilState,
                                    MethodOffset.RtControl,
                                    MethodOffset.RtDepthStencilSize,
                                    MethodOffset.RtDepthStencilEnable))
            {
                UpdateRenderTargetState(state, useControl: true);
            }

            if (state.QueryModified(MethodOffset.ScissorState))
            {
                UpdateScissorState(state);
            }

            if (state.QueryModified(MethodOffset.ViewVolumeClipControl))
            {
                UpdateDepthClampState(state);
            }

            if (state.QueryModified(MethodOffset.AlphaTestEnable,
                                    MethodOffset.AlphaTestRef,
                                    MethodOffset.AlphaTestFunc))
            {
                UpdateAlphaTestState(state);
            }

            if (state.QueryModified(MethodOffset.DepthTestEnable,
                                    MethodOffset.DepthWriteEnable,
                                    MethodOffset.DepthTestFunc))
            {
                UpdateDepthTestState(state);
            }

            if (state.QueryModified(MethodOffset.DepthMode,
                                    MethodOffset.ViewportTransform,
                                    MethodOffset.ViewportExtents))
            {
                UpdateViewportTransform(state);
            }

            if (state.QueryModified(MethodOffset.DepthBiasState,
                                    MethodOffset.DepthBiasFactor,
                                    MethodOffset.DepthBiasUnits,
                                    MethodOffset.DepthBiasClamp))
            {
                UpdateDepthBiasState(state);
            }

            if (state.QueryModified(MethodOffset.StencilBackMasks,
                                    MethodOffset.StencilTestState,
                                    MethodOffset.StencilBackTestState))
            {
                UpdateStencilTestState(state);
            }

            // Pools.
            if (state.QueryModified(MethodOffset.SamplerPoolState, MethodOffset.SamplerIndex))
            {
                UpdateSamplerPoolState(state);
            }

            if (state.QueryModified(MethodOffset.TexturePoolState))
            {
                UpdateTexturePoolState(state);
            }

            // Input assembler state.
            if (state.QueryModified(MethodOffset.VertexAttribState))
            {
                UpdateVertexAttribState(state);
            }

            if (state.QueryModified(MethodOffset.PointSize,
                                    MethodOffset.VertexProgramPointSize,
                                    MethodOffset.PointSpriteEnable,
                                    MethodOffset.PointCoordReplace))
            {
                UpdatePointState(state);
            }

            if (state.QueryModified(MethodOffset.PrimitiveRestartState))
            {
                UpdatePrimitiveRestartState(state);
            }

            if (state.QueryModified(MethodOffset.IndexBufferState))
            {
                UpdateIndexBufferState(state, firstIndex, indexCount);
            }

            if (state.QueryModified(MethodOffset.VertexBufferDrawState,
                                    MethodOffset.VertexBufferInstanced,
                                    MethodOffset.VertexBufferState,
                                    MethodOffset.VertexBufferEndAddress))
            {
                UpdateVertexBufferState(state);
            }

            if (state.QueryModified(MethodOffset.FaceState))
            {
                UpdateFaceState(state);
            }

            if (state.QueryModified(MethodOffset.RtColorMaskShared, MethodOffset.RtColorMask))
            {
                UpdateRtColorMask(state);
            }

            if (state.QueryModified(MethodOffset.BlendIndependent,
                                    MethodOffset.BlendConstant,
                                    MethodOffset.BlendStateCommon,
                                    MethodOffset.BlendEnableCommon,
                                    MethodOffset.BlendEnable,
                                    MethodOffset.BlendState))
            {
                UpdateBlendState(state);
            }

            if (state.QueryModified(MethodOffset.LogicOpState))
            {
                UpdateLogicOpState(state);
            }

            CommitBindings();

            if (tfEnable && !_prevTfEnable)
            {
                _context.Renderer.Pipeline.BeginTransformFeedback(Topology);
                _prevTfEnable = true;
            }
        }

        /// <summary>
        /// Updates Rasterizer primitive discard state based on guest gpu state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateRasterizerState(GpuState state)
        {
            Boolean32 enable = state.Get<Boolean32>(MethodOffset.RasterizeEnable);
            _context.Renderer.Pipeline.SetRasterizerDiscard(!enable);
        }

        /// <summary>
        /// Ensures that the bindings are visible to the host GPU.
        /// Note: this actually performs the binding using the host graphics API.
        /// </summary>
        private void CommitBindings()
        {
            UpdateStorageBuffers();

            BufferManager.CommitGraphicsBindings();
            TextureManager.CommitGraphicsBindings();
        }

        /// <summary>
        /// Updates storage buffer bindings.
        /// </summary>
        private void UpdateStorageBuffers()
        {
            for (int stage = 0; stage < _currentProgramInfo.Length; stage++)
            {
                ShaderProgramInfo info = _currentProgramInfo[stage];

                if (info == null)
                {
                    continue;
                }

                for (int index = 0; index < info.SBuffers.Count; index++)
                {
                    BufferDescriptor sb = info.SBuffers[index];

                    ulong sbDescAddress = BufferManager.GetGraphicsUniformBufferAddress(stage, 0);

                    int sbDescOffset = 0x110 + stage * 0x100 + sb.Slot * 0x10;

                    sbDescAddress += (ulong)sbDescOffset;

                    SbDescriptor sbDescriptor = _context.PhysicalMemory.Read<SbDescriptor>(sbDescAddress);

                    BufferManager.SetGraphicsStorageBuffer(stage, sb.Slot, sbDescriptor.PackAddress(), (uint)sbDescriptor.Size, sb.Flags);
                }
            }
        }

        /// <summary>
        /// Updates render targets (color and depth-stencil buffers) based on current render target state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="useControl">Use draw buffers information from render target control register</param>
        /// <param name="singleUse">If this is not -1, it indicates that only the given indexed target will be used.</param>
        private void UpdateRenderTargetState(GpuState state, bool useControl, int singleUse = -1)
        {
            var rtControl = state.Get<RtControl>(MethodOffset.RtControl);

            int count = useControl ? rtControl.UnpackCount() : Constants.TotalRenderTargets;

            var msaaMode = state.Get<TextureMsaaMode>(MethodOffset.RtMsaaMode);

            int samplesInX = msaaMode.SamplesInX();
            int samplesInY = msaaMode.SamplesInY();

            var scissor = state.Get<ScreenScissorState>(MethodOffset.ScreenScissorState);
            Size sizeHint = new Size(scissor.X + scissor.Width, scissor.Y + scissor.Height, 1);

            bool changedScale = false;

            for (int index = 0; index < Constants.TotalRenderTargets; index++)
            {
                int rtIndex = useControl ? rtControl.UnpackPermutationIndex(index) : index;

                var colorState = state.Get<RtColorState>(MethodOffset.RtColorState, rtIndex);

                if (index >= count || !IsRtEnabled(colorState))
                {
                    changedScale |= TextureManager.SetRenderTargetColor(index, null);

                    continue;
                }

                Texture color = TextureManager.FindOrCreateTexture(colorState, samplesInX, samplesInY, sizeHint);

                changedScale |= TextureManager.SetRenderTargetColor(index, color);
            }

            bool dsEnable = state.Get<Boolean32>(MethodOffset.RtDepthStencilEnable);

            Texture depthStencil = null;

            if (dsEnable)
            {
                var dsState = state.Get<RtDepthStencilState>(MethodOffset.RtDepthStencilState);
                var dsSize  = state.Get<Size3D>(MethodOffset.RtDepthStencilSize);

                depthStencil = TextureManager.FindOrCreateTexture(dsState, dsSize, samplesInX, samplesInY, sizeHint);
            }

            changedScale |= TextureManager.SetRenderTargetDepthStencil(depthStencil);

            if (changedScale)
            {
                TextureManager.UpdateRenderTargetScale(singleUse);
                _context.Renderer.Pipeline.SetRenderTargetScale(TextureManager.RenderTargetScale);

                UpdateViewportTransform(state);
                UpdateScissorState(state);
            }
        }

        /// <summary>
        /// Checks if a render target color buffer is used.
        /// </summary>
        /// <param name="colorState">Color buffer information</param>
        /// <returns>True if the specified buffer is enabled/used, false otherwise</returns>
        private static bool IsRtEnabled(RtColorState colorState)
        {
            // Colors are disabled by writing 0 to the format.
            return colorState.Format != 0 && colorState.WidthOrStride != 0;
        }

        /// <summary>
        /// Updates host scissor test state based on current GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateScissorState(GpuState state)
        {
            for (int index = 0; index < Constants.TotalViewports; index++)
            {
                ScissorState scissor = state.Get<ScissorState>(MethodOffset.ScissorState, index);

                bool enable = scissor.Enable && (scissor.X1 != 0 || scissor.Y1 != 0 || scissor.X2 != 0xffff || scissor.Y2 != 0xffff);

                if (enable)
                {
                    int x = scissor.X1;
                    int y = scissor.Y1;
                    int width = scissor.X2 - x;
                    int height = scissor.Y2 - y;

                    float scale = TextureManager.RenderTargetScale;
                    if (scale != 1f)
                    {
                        x = (int)(x * scale);
                        y = (int)(y * scale);
                        width = (int)Math.Ceiling(width * scale);
                        height = (int)Math.Ceiling(height * scale);
                    }

                    _context.Renderer.Pipeline.SetScissor(index, true, x, y, width, height);
                }
                else
                {
                    _context.Renderer.Pipeline.SetScissor(index, false, 0, 0, 0, 0);
                }
            }
        }

        /// <summary>
        /// Updates host depth clamp state based on current GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateDepthClampState(GpuState state)
        {
            ViewVolumeClipControl clip = state.Get<ViewVolumeClipControl>(MethodOffset.ViewVolumeClipControl);
            _context.Renderer.Pipeline.SetDepthClamp((clip & ViewVolumeClipControl.DepthClampDisabled) == 0);
        }

        /// <summary>
        /// Updates host alpha test state based on current GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateAlphaTestState(GpuState state)
        {
            _context.Renderer.Pipeline.SetAlphaTest(
                state.Get<Boolean32>(MethodOffset.AlphaTestEnable),
                state.Get<float>(MethodOffset.AlphaTestRef),
                state.Get<CompareOp>(MethodOffset.AlphaTestFunc));
        }

        /// <summary>
        /// Updates host depth test state based on current GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateDepthTestState(GpuState state)
        {
            _context.Renderer.Pipeline.SetDepthTest(new DepthTestDescriptor(
                state.Get<Boolean32>(MethodOffset.DepthTestEnable),
                state.Get<Boolean32>(MethodOffset.DepthWriteEnable),
                state.Get<CompareOp>(MethodOffset.DepthTestFunc)));
        }

        /// <summary>
        /// Updates host viewport transform and clipping state based on current GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateViewportTransform(GpuState state)
        {
            var yControl = state.Get<YControl> (MethodOffset.YControl);
            var face     = state.Get<FaceState>(MethodOffset.FaceState);

            UpdateFrontFace(yControl, face.FrontFace);

            bool flipY = yControl.HasFlag(YControl.NegateY);

            Span<Viewport> viewports = stackalloc Viewport[Constants.TotalViewports];

            for (int index = 0; index < Constants.TotalViewports; index++)
            {
                var transform = state.Get<ViewportTransform>(MethodOffset.ViewportTransform, index);
                var extents   = state.Get<ViewportExtents>  (MethodOffset.ViewportExtents,   index);

                float scaleX = MathF.Abs(transform.ScaleX);
                float scaleY = transform.ScaleY;

                if (flipY)
                {
                    scaleY = -scaleY;
                }

                if (!_context.Capabilities.SupportsViewportSwizzle && transform.UnpackSwizzleY() == ViewportSwizzle.NegativeY)
                {
                    scaleY = -scaleY;
                }

                if (index == 0)
                {
                    // Try to guess the depth mode being used on the high level API
                    // based on current transform.
                    // It is setup like so by said APIs:
                    // If depth mode is ZeroToOne:
                    //  TranslateZ = Near
                    //  ScaleZ = Far - Near
                    // If depth mode is MinusOneToOne:
                    //  TranslateZ = (Near + Far) / 2
                    //  ScaleZ = (Far - Near) / 2
                    // DepthNear/Far are sorted such as that Near is always less than Far.
                    DepthMode depthMode = extents.DepthNear != transform.TranslateZ &&
                                          extents.DepthFar  != transform.TranslateZ ? DepthMode.MinusOneToOne : DepthMode.ZeroToOne;

                    _context.Renderer.Pipeline.SetDepthMode(depthMode);
                }

                float x = transform.TranslateX - scaleX;
                float y = transform.TranslateY - scaleY;

                float width  = scaleX * 2;
                float height = scaleY * 2;

                float scale = TextureManager.RenderTargetScale;
                if (scale != 1f)
                {
                    x *= scale;
                    y *= scale;
                    width *= scale;
                    height *= scale;
                }

                RectangleF region = new RectangleF(x, y, width, height);

                ViewportSwizzle swizzleX = transform.UnpackSwizzleX();
                ViewportSwizzle swizzleY = transform.UnpackSwizzleY();
                ViewportSwizzle swizzleZ = transform.UnpackSwizzleZ();
                ViewportSwizzle swizzleW = transform.UnpackSwizzleW();

                float depthNear = extents.DepthNear;
                float depthFar  = extents.DepthFar;

                if (transform.ScaleZ < 0)
                {
                    float temp = depthNear;
                    depthNear  = depthFar;
                    depthFar   = temp;
                }

                viewports[index] = new Viewport(region, swizzleX, swizzleY, swizzleZ, swizzleW, depthNear, depthFar);
            }

            _context.Renderer.Pipeline.SetViewports(0, viewports);
        }

        /// <summary>
        /// Updates host depth bias (also called polygon offset) state based on current GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateDepthBiasState(GpuState state)
        {
            var depthBias = state.Get<DepthBiasState>(MethodOffset.DepthBiasState);

            float factor = state.Get<float>(MethodOffset.DepthBiasFactor);
            float units  = state.Get<float>(MethodOffset.DepthBiasUnits);
            float clamp  = state.Get<float>(MethodOffset.DepthBiasClamp);

            PolygonModeMask enables;

            enables  = (depthBias.PointEnable ? PolygonModeMask.Point : 0);
            enables |= (depthBias.LineEnable  ? PolygonModeMask.Line  : 0);
            enables |= (depthBias.FillEnable  ? PolygonModeMask.Fill  : 0);

            _context.Renderer.Pipeline.SetDepthBias(enables, factor, units / 2f, clamp);
        }

        /// <summary>
        /// Updates host stencil test state based on current GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateStencilTestState(GpuState state)
        {
            var backMasks = state.Get<StencilBackMasks>(MethodOffset.StencilBackMasks);
            var test      = state.Get<StencilTestState>(MethodOffset.StencilTestState);
            var backTest  = state.Get<StencilBackTestState>(MethodOffset.StencilBackTestState);

            CompareOp backFunc;
            StencilOp backSFail;
            StencilOp backDpPass;
            StencilOp backDpFail;
            int       backFuncRef;
            int       backFuncMask;
            int       backMask;

            if (backTest.TwoSided)
            {
                backFunc     = backTest.BackFunc;
                backSFail    = backTest.BackSFail;
                backDpPass   = backTest.BackDpPass;
                backDpFail   = backTest.BackDpFail;
                backFuncRef  = backMasks.FuncRef;
                backFuncMask = backMasks.FuncMask;
                backMask     = backMasks.Mask;
            }
            else
            {
                backFunc     = test.FrontFunc;
                backSFail    = test.FrontSFail;
                backDpPass   = test.FrontDpPass;
                backDpFail   = test.FrontDpFail;
                backFuncRef  = test.FrontFuncRef;
                backFuncMask = test.FrontFuncMask;
                backMask     = test.FrontMask;
            }

            _context.Renderer.Pipeline.SetStencilTest(new StencilTestDescriptor(
                test.Enable,
                test.FrontFunc,
                test.FrontSFail,
                test.FrontDpPass,
                test.FrontDpFail,
                test.FrontFuncRef,
                test.FrontFuncMask,
                test.FrontMask,
                backFunc,
                backSFail,
                backDpPass,
                backDpFail,
                backFuncRef,
                backFuncMask,
                backMask));
        }

        /// <summary>
        /// Updates current sampler pool address and size based on guest GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateSamplerPoolState(GpuState state)
        {
            var texturePool = state.Get<PoolState>(MethodOffset.TexturePoolState);
            var samplerPool = state.Get<PoolState>(MethodOffset.SamplerPoolState);

            var samplerIndex = state.Get<SamplerIndex>(MethodOffset.SamplerIndex);

            int maximumId = samplerIndex == SamplerIndex.ViaHeaderIndex
                ? texturePool.MaximumId
                : samplerPool.MaximumId;

            TextureManager.SetGraphicsSamplerPool(samplerPool.Address.Pack(), maximumId, samplerIndex);
        }

        /// <summary>
        /// Updates current texture pool address and size based on guest GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateTexturePoolState(GpuState state)
        {
            var texturePool = state.Get<PoolState>(MethodOffset.TexturePoolState);

            TextureManager.SetGraphicsTexturePool(texturePool.Address.Pack(), texturePool.MaximumId);

            TextureManager.SetGraphicsTextureBufferIndex(state.Get<int>(MethodOffset.TextureBufferIndex));
        }

        /// <summary>
        /// Updates host vertex attributes based on guest GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateVertexAttribState(GpuState state)
        {
            Span<VertexAttribDescriptor> vertexAttribs = stackalloc VertexAttribDescriptor[Constants.TotalVertexAttribs];

            for (int index = 0; index < Constants.TotalVertexAttribs; index++)
            {
                var vertexAttrib = state.Get<VertexAttribState>(MethodOffset.VertexAttribState, index);

                if (!FormatTable.TryGetAttribFormat(vertexAttrib.UnpackFormat(), out Format format))
                {
                    Logger.Debug?.Print(LogClass.Gpu, $"Invalid attribute format 0x{vertexAttrib.UnpackFormat():X}.");

                    format = Format.R32G32B32A32Float;
                }

                vertexAttribs[index] = new VertexAttribDescriptor(
                    vertexAttrib.UnpackBufferIndex(),
                    vertexAttrib.UnpackOffset(),
                    vertexAttrib.UnpackIsConstant(),
                    format);
            }

            _context.Renderer.Pipeline.SetVertexAttribs(vertexAttribs);
        }

        /// <summary>
        /// Updates host point size based on guest GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdatePointState(GpuState state)
        {
            float size = state.Get<float>(MethodOffset.PointSize);
            bool isProgramPointSize = state.Get<Boolean32>(MethodOffset.VertexProgramPointSize);
            bool enablePointSprite = state.Get<Boolean32>(MethodOffset.PointSpriteEnable);

            // TODO: Need to figure out a way to map PointCoordReplace enable bit.
            Origin origin = (state.Get<int>(MethodOffset.PointCoordReplace) & 4) == 0 ? Origin.LowerLeft : Origin.UpperLeft;

            _context.Renderer.Pipeline.SetPointParameters(size, isProgramPointSize, enablePointSprite, origin);
        }

        /// <summary>
        /// Updates host primitive restart based on guest GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdatePrimitiveRestartState(GpuState state)
        {
            PrimitiveRestartState primitiveRestart = state.Get<PrimitiveRestartState>(MethodOffset.PrimitiveRestartState);

            _context.Renderer.Pipeline.SetPrimitiveRestart(
                primitiveRestart.Enable,
                primitiveRestart.Index);
        }

        /// <summary>
        /// Updates host index buffer binding based on guest GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="firstIndex">Index of the first index buffer element used on the draw</param>
        /// <param name="indexCount">Number of index buffer elements used on the draw</param>
        private void UpdateIndexBufferState(GpuState state, int firstIndex, int indexCount)
        {
            var indexBuffer = state.Get<IndexBufferState>(MethodOffset.IndexBufferState);

            if (indexCount == 0)
            {
                return;
            }

            ulong gpuVa = indexBuffer.Address.Pack();

            // Do not use the end address to calculate the size, because
            // the result may be much larger than the real size of the index buffer.
            ulong size = (ulong)(firstIndex + indexCount);

            switch (indexBuffer.Type)
            {
                case IndexType.UShort: size *= 2; break;
                case IndexType.UInt:   size *= 4; break;
            }

            BufferManager.SetIndexBuffer(gpuVa, size, indexBuffer.Type);

            // The index buffer affects the vertex buffer size calculation, we
            // need to ensure that they are updated.
            UpdateVertexBufferState(state);
        }

        /// <summary>
        /// Updates host vertex buffer bindings based on guest GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateVertexBufferState(GpuState state)
        {
            _isAnyVbInstanced = false;

            for (int index = 0; index < Constants.TotalVertexBuffers; index++)
            {
                var vertexBuffer = state.Get<VertexBufferState>(MethodOffset.VertexBufferState, index);

                if (!vertexBuffer.UnpackEnable())
                {
                    BufferManager.SetVertexBuffer(index, 0, 0, 0, 0);

                    continue;
                }

                GpuVa endAddress = state.Get<GpuVa>(MethodOffset.VertexBufferEndAddress, index);

                ulong address = vertexBuffer.Address.Pack();

                int stride = vertexBuffer.UnpackStride();

                bool instanced = state.Get<Boolean32>(MethodOffset.VertexBufferInstanced + index);

                int divisor = instanced ? vertexBuffer.Divisor : 0;

                _isAnyVbInstanced |= divisor != 0;

                ulong size;

                if (_ibStreamer.HasInlineIndexData || _drawIndexed || stride == 0 || instanced)
                {
                    // This size may be (much) larger than the real vertex buffer size.
                    // Avoid calculating it this way, unless we don't have any other option.
                    size = endAddress.Pack() - address + 1;
                }
                else
                {
                    // For non-indexed draws, we can guess the size from the vertex count
                    // and stride.
                    int firstInstance = state.Get<int>(MethodOffset.FirstInstance);

                    var drawState = state.Get<VertexBufferDrawState>(MethodOffset.VertexBufferDrawState);

                    size = (ulong)((firstInstance + drawState.First + drawState.Count) * stride);
                }

                BufferManager.SetVertexBuffer(index, address, size, stride, divisor);
            }
        }

        /// <summary>
        /// Updates host face culling and orientation based on guest GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateFaceState(GpuState state)
        {
            var yControl = state.Get<YControl> (MethodOffset.YControl);
            var face     = state.Get<FaceState>(MethodOffset.FaceState);

            _context.Renderer.Pipeline.SetFaceCulling(face.CullEnable, face.CullFace);

            UpdateFrontFace(yControl, face.FrontFace);
        }

        /// <summary>
        /// Updates the front face based on the current front face and the origin.
        /// </summary>
        /// <param name="yControl">Y control register value, where the origin is located</param>
        /// <param name="frontFace">Front face</param>
        private void UpdateFrontFace(YControl yControl, FrontFace frontFace)
        {
            bool isUpperLeftOrigin = !yControl.HasFlag(YControl.TriangleRastFlip);

            if (isUpperLeftOrigin)
            {
                frontFace = frontFace == FrontFace.CounterClockwise ? FrontFace.Clockwise : FrontFace.CounterClockwise;
            }

            _context.Renderer.Pipeline.SetFrontFace(frontFace);
        }

        /// <summary>
        /// Updates host render target color masks, based on guest GPU state.
        /// This defines which color channels are written to each color buffer.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateRtColorMask(GpuState state)
        {
            bool rtColorMaskShared = state.Get<Boolean32>(MethodOffset.RtColorMaskShared);

            Span<uint> componentMasks = stackalloc uint[Constants.TotalRenderTargets];

            for (int index = 0; index < Constants.TotalRenderTargets; index++)
            {
                var colorMask = state.Get<RtColorMask>(MethodOffset.RtColorMask, rtColorMaskShared ? 0 : index);

                uint componentMask;

                componentMask  = (colorMask.UnpackRed()   ? 1u : 0u);
                componentMask |= (colorMask.UnpackGreen() ? 2u : 0u);
                componentMask |= (colorMask.UnpackBlue()  ? 4u : 0u);
                componentMask |= (colorMask.UnpackAlpha() ? 8u : 0u);

                componentMasks[index] = componentMask;
            }

            _context.Renderer.Pipeline.SetRenderTargetColorMasks(componentMasks);
        }

        /// <summary>
        /// Updates host render target color buffer blending state, based on guest state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateBlendState(GpuState state)
        {
            bool blendIndependent = state.Get<Boolean32>(MethodOffset.BlendIndependent);
            ColorF blendConstant = state.Get<ColorF>(MethodOffset.BlendConstant);

            for (int index = 0; index < Constants.TotalRenderTargets; index++)
            {
                BlendDescriptor descriptor;

                if (blendIndependent)
                {
                    bool enable = state.Get<Boolean32> (MethodOffset.BlendEnable, index);
                    var  blend  = state.Get<BlendState>(MethodOffset.BlendState,  index);

                    descriptor = new BlendDescriptor(
                        enable,
                        blendConstant,
                        blend.ColorOp,
                        blend.ColorSrcFactor,
                        blend.ColorDstFactor,
                        blend.AlphaOp,
                        blend.AlphaSrcFactor,
                        blend.AlphaDstFactor);
                }
                else
                {
                    bool enable = state.Get<Boolean32>       (MethodOffset.BlendEnable, 0);
                    var  blend  = state.Get<BlendStateCommon>(MethodOffset.BlendStateCommon);

                    descriptor = new BlendDescriptor(
                        enable,
                        blendConstant,
                        blend.ColorOp,
                        blend.ColorSrcFactor,
                        blend.ColorDstFactor,
                        blend.AlphaOp,
                        blend.AlphaSrcFactor,
                        blend.AlphaDstFactor);
                }

                _context.Renderer.Pipeline.SetBlendState(index, descriptor);
            }
        }

        /// <summary>
        /// Updates host logical operation state, based on guest state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        public void UpdateLogicOpState(GpuState state)
        {
            LogicalOpState logicOpState = state.Get<LogicalOpState>(MethodOffset.LogicOpState);

            _context.Renderer.Pipeline.SetLogicOpState(logicOpState.Enable, logicOpState.LogicalOp);
        }

        /// <summary>
        /// Storage buffer address and size information.
        /// </summary>
        private struct SbDescriptor
        {
#pragma warning disable CS0649
            public uint AddressLow;
            public uint AddressHigh;
            public int  Size;
            public int  Padding;
#pragma warning restore CS0649

            public ulong PackAddress()
            {
                return AddressLow | ((ulong)AddressHigh << 32);
            }
        }

        /// <summary>
        /// Updates host shaders based on the guest GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateShaderState(GpuState state)
        {
            ShaderAddresses addresses = new ShaderAddresses();

            Span<ShaderAddresses> addressesSpan = MemoryMarshal.CreateSpan(ref addresses, 1);

            Span<ulong> addressesArray = MemoryMarshal.Cast<ShaderAddresses, ulong>(addressesSpan);

            ulong baseAddress = state.Get<GpuVa>(MethodOffset.ShaderBaseAddress).Pack();

            for (int index = 0; index < 6; index++)
            {
                var shader = state.Get<ShaderState>(MethodOffset.ShaderState, index);

                if (!shader.UnpackEnable() && index != 1)
                {
                    continue;
                }

                addressesArray[index] = baseAddress + shader.Offset;
            }

            ShaderBundle gs = ShaderCache.GetGraphicsShader(state, addresses);

            _vsUsesInstanceId = gs.Shaders[0]?.Info.UsesInstanceId ?? false;

            int storageBufferBindingsCount = 0;
            int uniformBufferBindingsCount = 0;

            for (int stage = 0; stage < Constants.ShaderStages; stage++)
            {
                ShaderProgramInfo info = gs.Shaders[stage]?.Info;

                _currentProgramInfo[stage] = info;

                if (info == null)
                {
                    TextureManager.SetGraphicsTextures(stage, Array.Empty<TextureBindingInfo>());
                    TextureManager.SetGraphicsImages(stage, Array.Empty<TextureBindingInfo>());
                    BufferManager.SetGraphicsStorageBufferBindings(stage, null);
                    BufferManager.SetGraphicsUniformBufferBindings(stage, null);
                    continue;
                }

                var textureBindings = new TextureBindingInfo[info.Textures.Count];

                for (int index = 0; index < info.Textures.Count; index++)
                {
                    var descriptor = info.Textures[index];

                    Target target = ShaderTexture.GetTarget(descriptor.Type);

                    textureBindings[index] = new TextureBindingInfo(
                        target,
                        descriptor.Binding,
                        descriptor.CbufSlot,
                        descriptor.HandleIndex,
                        descriptor.Flags);
                }

                TextureManager.SetGraphicsTextures(stage, textureBindings);

                var imageBindings = new TextureBindingInfo[info.Images.Count];

                for (int index = 0; index < info.Images.Count; index++)
                {
                    var descriptor = info.Images[index];

                    Target target = ShaderTexture.GetTarget(descriptor.Type);
                    Format format = ShaderTexture.GetFormat(descriptor.Format);

                    imageBindings[index] = new TextureBindingInfo(
                        target,
                        format,
                        descriptor.Binding,
                        descriptor.CbufSlot,
                        descriptor.HandleIndex,
                        descriptor.Flags);
                }

                TextureManager.SetGraphicsImages(stage, imageBindings);

                BufferManager.SetGraphicsStorageBufferBindings(stage, info.SBuffers);
                BufferManager.SetGraphicsUniformBufferBindings(stage, info.CBuffers);

                if (info.SBuffers.Count != 0)
                {
                    storageBufferBindingsCount = Math.Max(storageBufferBindingsCount, info.SBuffers.Max(x => x.Binding) + 1);
                }

                if (info.CBuffers.Count != 0)
                {
                    uniformBufferBindingsCount = Math.Max(uniformBufferBindingsCount, info.CBuffers.Max(x => x.Binding) + 1);
                }
            }

            BufferManager.SetGraphicsStorageBufferBindingsCount(storageBufferBindingsCount);
            BufferManager.SetGraphicsUniformBufferBindingsCount(uniformBufferBindingsCount);

            _context.Renderer.Pipeline.SetProgram(gs.HostProgram);
        }

        /// <summary>
        /// Updates transform feedback buffer state based on the guest GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateTfBufferState(GpuState state)
        {
            for (int index = 0; index < Constants.TotalTransformFeedbackBuffers; index++)
            {
                TfBufferState tfb = state.Get<TfBufferState>(MethodOffset.TfBufferState, index);

                if (!tfb.Enable)
                {
                    BufferManager.SetTransformFeedbackBuffer(index, 0, 0);

                    continue;
                }

                BufferManager.SetTransformFeedbackBuffer(index, tfb.Address.Pack(), (uint)tfb.Size);
            }
        }

        /// <summary>
        /// Updates user-defined clipping based on the guest GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        private void UpdateUserClipState(GpuState state)
        {
            int clipMask = state.Get<int>(MethodOffset.ClipDistanceEnable);

            for (int i = 0; i < Constants.TotalClipDistances; ++i)
            {
                _context.Renderer.Pipeline.SetUserClipDistance(i, (clipMask & (1 << i)) != 0);
            }
        }

        /// <summary>
        /// Issues a texture barrier.
        /// This waits until previous texture writes from the GPU to finish, before
        /// performing new operations with said textures.
        /// </summary>
        /// <param name="state">Current GPU state (unused)</param>
        /// <param name="argument">Method call argument (unused)</param>
        private void TextureBarrier(GpuState state, int argument)
        {
            _context.Renderer.Pipeline.TextureBarrier();
        }

        /// <summary>
        /// Issues a texture barrier.
        /// This waits until previous texture writes from the GPU to finish, before
        /// performing new operations with said textures.
        /// This performs a per-tile wait, it is only valid if both the previous write
        /// and current access has the same access patterns.
        /// This may be faster than the regular barrier on tile-based rasterizers.
        /// </summary>
        /// <param name="state">Current GPU state (unused)</param>
        /// <param name="argument">Method call argument (unused)</param>
        private void TextureBarrierTiled(GpuState state, int argument)
        {
            _context.Renderer.Pipeline.TextureBarrierTiled();
        }
    }
}
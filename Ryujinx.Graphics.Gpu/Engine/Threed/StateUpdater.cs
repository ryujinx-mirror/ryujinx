using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.Types;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.Shader;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Texture;
using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.Gpu.Engine.Threed
{
    /// <summary>
    /// GPU state updater.
    /// </summary>
    class StateUpdater
    {
        public const int ShaderStateIndex = 0;
        public const int RasterizerStateIndex = 1;
        public const int ScissorStateIndex = 2;
        public const int VertexBufferStateIndex = 3;
        public const int PrimitiveRestartStateIndex = 4;

        private readonly GpuContext _context;
        private readonly GpuChannel _channel;
        private readonly DeviceStateWithShadow<ThreedClassState> _state;
        private readonly DrawState _drawState;

        private readonly StateUpdateTracker<ThreedClassState> _updateTracker;

        private readonly ShaderProgramInfo[] _currentProgramInfo;
        private ShaderSpecializationState _shaderSpecState;

        private bool _vtgWritesRtLayer;
        private byte _vsClipDistancesWritten;

        private bool _prevDrawIndexed;
        private IndexType _prevIndexType;
        private uint _prevFirstVertex;
        private bool _prevTfEnable;

        /// <summary>
        /// Creates a new instance of the state updater.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="channel">GPU channel</param>
        /// <param name="state">3D engine state</param>
        /// <param name="drawState">Draw state</param>
        public StateUpdater(GpuContext context, GpuChannel channel, DeviceStateWithShadow<ThreedClassState> state, DrawState drawState)
        {
            _context = context;
            _channel = channel;
            _state = state;
            _drawState = drawState;
            _currentProgramInfo = new ShaderProgramInfo[Constants.ShaderStages];

            // ShaderState must be the first, as other state updates depends on information from the currently bound shader.
            // Rasterizer and scissor states are checked by render target clear, their indexes
            // must be updated on the constants "RasterizerStateIndex" and "ScissorStateIndex" if modified.
            // The vertex buffer state may be forced dirty when a indexed draw starts, the "VertexBufferStateIndex"
            // constant must be updated if modified.
            // The order of the other state updates doesn't matter.
            _updateTracker = new StateUpdateTracker<ThreedClassState>(new[]
            {
                new StateUpdateCallbackEntry(UpdateShaderState,
                    nameof(ThreedClassState.ShaderBaseAddress),
                    nameof(ThreedClassState.ShaderState)),

                new StateUpdateCallbackEntry(UpdateRasterizerState, nameof(ThreedClassState.RasterizeEnable)),

                new StateUpdateCallbackEntry(UpdateScissorState,
                    nameof(ThreedClassState.ScissorState),
                    nameof(ThreedClassState.ScreenScissorState)),

                new StateUpdateCallbackEntry(UpdateVertexBufferState,
                    nameof(ThreedClassState.VertexBufferDrawState),
                    nameof(ThreedClassState.VertexBufferInstanced),
                    nameof(ThreedClassState.VertexBufferState),
                    nameof(ThreedClassState.VertexBufferEndAddress)),

                new StateUpdateCallbackEntry(UpdatePrimitiveRestartState,
                    nameof(ThreedClassState.PrimitiveRestartDrawArrays),
                    nameof(ThreedClassState.PrimitiveRestartState)),

                new StateUpdateCallbackEntry(UpdateTessellationState,
                    nameof(ThreedClassState.TessOuterLevel),
                    nameof(ThreedClassState.TessInnerLevel),
                    nameof(ThreedClassState.PatchVertices)),

                new StateUpdateCallbackEntry(UpdateTfBufferState, nameof(ThreedClassState.TfBufferState)),
                new StateUpdateCallbackEntry(UpdateUserClipState, nameof(ThreedClassState.ClipDistanceEnable)),

                new StateUpdateCallbackEntry(UpdateRenderTargetState,
                    nameof(ThreedClassState.RtColorState),
                    nameof(ThreedClassState.RtDepthStencilState),
                    nameof(ThreedClassState.RtControl),
                    nameof(ThreedClassState.RtDepthStencilSize),
                    nameof(ThreedClassState.RtDepthStencilEnable)),

                new StateUpdateCallbackEntry(UpdateDepthClampState, nameof(ThreedClassState.ViewVolumeClipControl)),

                new StateUpdateCallbackEntry(UpdateAlphaTestState,
                    nameof(ThreedClassState.AlphaTestEnable),
                    nameof(ThreedClassState.AlphaTestRef),
                    nameof(ThreedClassState.AlphaTestFunc)),

                new StateUpdateCallbackEntry(UpdateDepthTestState,
                    nameof(ThreedClassState.DepthTestEnable),
                    nameof(ThreedClassState.DepthWriteEnable),
                    nameof(ThreedClassState.DepthTestFunc)),

                new StateUpdateCallbackEntry(UpdateViewportTransform,
                    nameof(ThreedClassState.DepthMode),
                    nameof(ThreedClassState.ViewportTransform),
                    nameof(ThreedClassState.ViewportExtents),
                    nameof(ThreedClassState.YControl)),

                new StateUpdateCallbackEntry(UpdatePolygonMode,
                    nameof(ThreedClassState.PolygonModeFront),
                    nameof(ThreedClassState.PolygonModeBack)),

                new StateUpdateCallbackEntry(UpdateDepthBiasState,
                    nameof(ThreedClassState.DepthBiasState),
                    nameof(ThreedClassState.DepthBiasFactor),
                    nameof(ThreedClassState.DepthBiasUnits),
                    nameof(ThreedClassState.DepthBiasClamp)),

                new StateUpdateCallbackEntry(UpdateStencilTestState,
                    nameof(ThreedClassState.StencilBackMasks),
                    nameof(ThreedClassState.StencilTestState),
                    nameof(ThreedClassState.StencilBackTestState)),

                new StateUpdateCallbackEntry(UpdateSamplerPoolState,
                    nameof(ThreedClassState.SamplerPoolState),
                    nameof(ThreedClassState.SamplerIndex)),

                new StateUpdateCallbackEntry(UpdateTexturePoolState, nameof(ThreedClassState.TexturePoolState)),
                new StateUpdateCallbackEntry(UpdateVertexAttribState, nameof(ThreedClassState.VertexAttribState)),

                new StateUpdateCallbackEntry(UpdateLineState,
                    nameof(ThreedClassState.LineWidthSmooth),
                    nameof(ThreedClassState.LineSmoothEnable)),

                new StateUpdateCallbackEntry(UpdatePointState,
                    nameof(ThreedClassState.PointSize),
                    nameof(ThreedClassState.VertexProgramPointSize),
                    nameof(ThreedClassState.PointSpriteEnable),
                    nameof(ThreedClassState.PointCoordReplace)),

                new StateUpdateCallbackEntry(UpdateIndexBufferState,
                    nameof(ThreedClassState.IndexBufferState),
                    nameof(ThreedClassState.IndexBufferCount)),

                new StateUpdateCallbackEntry(UpdateFaceState, nameof(ThreedClassState.FaceState)),

                new StateUpdateCallbackEntry(UpdateRtColorMask,
                    nameof(ThreedClassState.RtColorMaskShared),
                    nameof(ThreedClassState.RtColorMask)),

                new StateUpdateCallbackEntry(UpdateBlendState,
                    nameof(ThreedClassState.BlendIndependent),
                    nameof(ThreedClassState.BlendConstant),
                    nameof(ThreedClassState.BlendStateCommon),
                    nameof(ThreedClassState.BlendEnableCommon),
                    nameof(ThreedClassState.BlendEnable),
                    nameof(ThreedClassState.BlendState)),

                new StateUpdateCallbackEntry(UpdateLogicOpState, nameof(ThreedClassState.LogicOpState))
            });
        }

        /// <summary>
        /// Sets a register at a specific offset as dirty.
        /// This must be called if the register value was modified.
        /// </summary>
        /// <param name="offset">Register offset</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetDirty(int offset)
        {
            _updateTracker.SetDirty(offset);
        }

        /// <summary>
        /// Force all the guest state to be marked as dirty.
        /// The next call to <see cref="Update"/> will update all the host state.
        /// </summary>
        public void SetAllDirty()
        {
            _updateTracker.SetAllDirty();
        }

        /// <summary>
        /// Updates host state for any modified guest state, since the last time this function was called.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update()
        {
            // If any state that the shader depends on changed,
            // then we may need to compile/bind a different version
            // of the shader for the new state.
            if (_shaderSpecState != null)
            {
                if (!_shaderSpecState.MatchesGraphics(_channel, GetPoolState()))
                {
                    ForceShaderUpdate();
                }
            }

            // The vertex buffer size is calculated using a different
            // method when doing indexed draws, so we need to make sure
            // to update the vertex buffers if we are doing a regular
            // draw after a indexed one and vice-versa.
            if (_drawState.DrawIndexed != _prevDrawIndexed)
            {
                _updateTracker.ForceDirty(VertexBufferStateIndex);

                // If PrimitiveRestartDrawArrays is false and this is a non-indexed draw, we need to ensure primitive restart is disabled.
                // If PrimitiveRestartDrawArrays is false and this is a indexed draw, we need to ensure primitive restart enable matches GPU state.
                // If PrimitiveRestartDrawArrays is true, then primitive restart enable should always match GPU state.
                // That is because "PrimitiveRestartDrawArrays" is not configurable on the backend, it is always
                // true on OpenGL and always false on Vulkan.
                if (!_state.State.PrimitiveRestartDrawArrays && _state.State.PrimitiveRestartState.Enable)
                {
                    _updateTracker.ForceDirty(PrimitiveRestartStateIndex);
                }

                _prevDrawIndexed = _drawState.DrawIndexed;
            }

            // In some cases, the index type is also used to guess the
            // vertex buffer size, so we must update it if the type changed too.
            if (_drawState.DrawIndexed &&
                (_prevIndexType != _state.State.IndexBufferState.Type ||
                 _prevFirstVertex != _state.State.FirstVertex))
            {
                _updateTracker.ForceDirty(VertexBufferStateIndex);
                _prevIndexType = _state.State.IndexBufferState.Type;
                _prevFirstVertex = _state.State.FirstVertex;
            }

            bool tfEnable = _state.State.TfEnable;

            if (!tfEnable && _prevTfEnable)
            {
                _context.Renderer.Pipeline.EndTransformFeedback();
                _prevTfEnable = false;
            }

            _updateTracker.Update(ulong.MaxValue);

            CommitBindings();

            if (tfEnable && !_prevTfEnable)
            {
                _context.Renderer.Pipeline.BeginTransformFeedback(_drawState.Topology);
                _prevTfEnable = true;
            }
        }

        /// <summary>
        /// Updates the host state for any modified guest state group with the respective bit set on <paramref name="mask"/>.
        /// </summary>
        /// <param name="mask">Mask, where each bit set corresponds to a group index that should be checked and updated</param>
        public void Update(ulong mask)
        {
            _updateTracker.Update(mask);
        }

        /// <summary>
        /// Ensures that the bindings are visible to the host GPU.
        /// Note: this actually performs the binding using the host graphics API.
        /// </summary>
        private void CommitBindings()
        {
            UpdateStorageBuffers();

            _channel.TextureManager.CommitGraphicsBindings();
            _channel.BufferManager.CommitGraphicsBindings();
        }

        /// <summary>
        /// Updates storage buffer bindings.
        /// </summary>
        private void UpdateStorageBuffers()
        {
            for (int stage = 0; stage < Constants.ShaderStages; stage++)
            {
                ShaderProgramInfo info = _currentProgramInfo[stage];

                if (info == null)
                {
                    continue;
                }

                for (int index = 0; index < info.SBuffers.Count; index++)
                {
                    BufferDescriptor sb = info.SBuffers[index];

                    ulong sbDescAddress = _channel.BufferManager.GetGraphicsUniformBufferAddress(stage, 0);

                    int sbDescOffset = 0x110 + stage * 0x100 + sb.Slot * 0x10;

                    sbDescAddress += (ulong)sbDescOffset;

                    SbDescriptor sbDescriptor = _channel.MemoryManager.Physical.Read<SbDescriptor>(sbDescAddress);

                    _channel.BufferManager.SetGraphicsStorageBuffer(stage, sb.Slot, sbDescriptor.PackAddress(), (uint)sbDescriptor.Size, sb.Flags);
                }
            }
        }

        /// <summary>
        /// Updates tessellation state based on the guest GPU state.
        /// </summary>
        private void UpdateTessellationState()
        {
            _context.Renderer.Pipeline.SetPatchParameters(
                _state.State.PatchVertices,
                _state.State.TessOuterLevel.ToSpan(),
                _state.State.TessInnerLevel.ToSpan());
        }

        /// <summary>
        /// Updates transform feedback buffer state based on the guest GPU state.
        /// </summary>
        private void UpdateTfBufferState()
        {
            for (int index = 0; index < Constants.TotalTransformFeedbackBuffers; index++)
            {
                TfBufferState tfb = _state.State.TfBufferState[index];

                if (!tfb.Enable)
                {
                    _channel.BufferManager.SetTransformFeedbackBuffer(index, 0, 0);

                    continue;
                }

                _channel.BufferManager.SetTransformFeedbackBuffer(index, tfb.Address.Pack(), (uint)tfb.Size);
            }
        }

        /// <summary>
        /// Updates Rasterizer primitive discard state based on guest gpu state.
        /// </summary>
        private void UpdateRasterizerState()
        {
            bool enable = _state.State.RasterizeEnable;
            _context.Renderer.Pipeline.SetRasterizerDiscard(!enable);
        }

        /// <summary>
        /// Updates render targets (color and depth-stencil buffers) based on current render target state.
        /// </summary>
        private void UpdateRenderTargetState()
        {
            UpdateRenderTargetState(true);
        }

        /// <summary>
        /// Updates render targets (color and depth-stencil buffers) based on current render target state.
        /// </summary>
        /// <param name="useControl">Use draw buffers information from render target control register</param>
        /// <param name="singleUse">If this is not -1, it indicates that only the given indexed target will be used.</param>
        public void UpdateRenderTargetState(bool useControl, int singleUse = -1)
        {
            var memoryManager = _channel.MemoryManager;
            var rtControl = _state.State.RtControl;

            int count = useControl ? rtControl.UnpackCount() : Constants.TotalRenderTargets;

            var msaaMode = _state.State.RtMsaaMode;

            int samplesInX = msaaMode.SamplesInX();
            int samplesInY = msaaMode.SamplesInY();

            var scissor = _state.State.ScreenScissorState;
            Size sizeHint = new Size(scissor.X + scissor.Width, scissor.Y + scissor.Height, 1);

            int clipRegionWidth = int.MaxValue;
            int clipRegionHeight = int.MaxValue;

            bool changedScale = false;

            for (int index = 0; index < Constants.TotalRenderTargets; index++)
            {
                int rtIndex = useControl ? rtControl.UnpackPermutationIndex(index) : index;

                var colorState = _state.State.RtColorState[rtIndex];

                if (index >= count || !IsRtEnabled(colorState))
                {
                    changedScale |= _channel.TextureManager.SetRenderTargetColor(index, null);

                    continue;
                }

                Image.Texture color = memoryManager.Physical.TextureCache.FindOrCreateTexture(
                    memoryManager,
                    colorState,
                    _vtgWritesRtLayer,
                    samplesInX,
                    samplesInY,
                    sizeHint);

                changedScale |= _channel.TextureManager.SetRenderTargetColor(index, color);

                if (color != null)
                {
                    if (clipRegionWidth > color.Width / samplesInX)
                    {
                        clipRegionWidth = color.Width / samplesInX;
                    }

                    if (clipRegionHeight > color.Height / samplesInY)
                    {
                        clipRegionHeight = color.Height / samplesInY;
                    }
                }
            }

            bool dsEnable = _state.State.RtDepthStencilEnable;

            Image.Texture depthStencil = null;

            if (dsEnable)
            {
                var dsState = _state.State.RtDepthStencilState;
                var dsSize = _state.State.RtDepthStencilSize;

                depthStencil = memoryManager.Physical.TextureCache.FindOrCreateTexture(
                    memoryManager,
                    dsState,
                    dsSize,
                    samplesInX,
                    samplesInY,
                    sizeHint);

                if (depthStencil != null)
                {
                    if (clipRegionWidth > depthStencil.Width / samplesInX)
                    {
                        clipRegionWidth = depthStencil.Width / samplesInX;
                    }

                    if (clipRegionHeight > depthStencil.Height / samplesInY)
                    {
                        clipRegionHeight = depthStencil.Height / samplesInY;
                    }
                }
            }

            changedScale |= _channel.TextureManager.SetRenderTargetDepthStencil(depthStencil);

            if (changedScale)
            {
                float oldScale = _channel.TextureManager.RenderTargetScale;
                _channel.TextureManager.UpdateRenderTargetScale(singleUse);

                if (oldScale != _channel.TextureManager.RenderTargetScale)
                {
                    _context.Renderer.Pipeline.SetRenderTargetScale(_channel.TextureManager.RenderTargetScale);

                    UpdateViewportTransform();
                    UpdateScissorState();
                }
            }

            _channel.TextureManager.SetClipRegion(clipRegionWidth, clipRegionHeight);
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
        public void UpdateScissorState()
        {
            for (int index = 0; index < Constants.TotalViewports; index++)
            {
                ScissorState scissor = _state.State.ScissorState[index];

                bool enable = scissor.Enable && (scissor.X1 != 0 || scissor.Y1 != 0 || scissor.X2 != 0xffff || scissor.Y2 != 0xffff);

                if (enable)
                {
                    int x = scissor.X1;
                    int y = scissor.Y1;
                    int width = scissor.X2 - x;
                    int height = scissor.Y2 - y;

                    if (_state.State.YControl.HasFlag(YControl.NegateY))
                    {
                        ref var screenScissor = ref _state.State.ScreenScissorState;
                        y = screenScissor.Height - height - y;

                        if (y < 0)
                        {
                            height += y;
                            y = 0;
                        }
                    }

                    float scale = _channel.TextureManager.RenderTargetScale;
                    if (scale != 1f)
                    {
                        x = (int)(x * scale);
                        y = (int)(y * scale);
                        width = (int)MathF.Ceiling(width * scale);
                        height = (int)MathF.Ceiling(height * scale);
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
        private void UpdateDepthClampState()
        {
            ViewVolumeClipControl clip = _state.State.ViewVolumeClipControl;
            _context.Renderer.Pipeline.SetDepthClamp((clip & ViewVolumeClipControl.DepthClampDisabled) == 0);
        }

        /// <summary>
        /// Updates host alpha test state based on current GPU state.
        /// </summary>
        private void UpdateAlphaTestState()
        {
            _context.Renderer.Pipeline.SetAlphaTest(
                _state.State.AlphaTestEnable,
                _state.State.AlphaTestRef,
                _state.State.AlphaTestFunc);
        }

        /// <summary>
        /// Updates host depth test state based on current GPU state.
        /// </summary>
        private void UpdateDepthTestState()
        {
            _context.Renderer.Pipeline.SetDepthTest(new DepthTestDescriptor(
                _state.State.DepthTestEnable,
                _state.State.DepthWriteEnable,
                _state.State.DepthTestFunc));
        }

        /// <summary>
        /// Updates host viewport transform and clipping state based on current GPU state.
        /// </summary>
        private void UpdateViewportTransform()
        {
            var yControl = _state.State.YControl;
            var face = _state.State.FaceState;

            UpdateFrontFace(yControl, face.FrontFace);
            UpdateDepthMode();

            bool flipY = yControl.HasFlag(YControl.NegateY);

            Span<Viewport> viewports = stackalloc Viewport[Constants.TotalViewports];

            for (int index = 0; index < Constants.TotalViewports; index++)
            {
                ref var transform = ref _state.State.ViewportTransform[index];
                ref var extents = ref _state.State.ViewportExtents[index];

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

                float x = transform.TranslateX - scaleX;
                float y = transform.TranslateY - scaleY;

                float width = scaleX * 2;
                float height = scaleY * 2;

                float scale = _channel.TextureManager.RenderTargetScale;
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
                float depthFar = extents.DepthFar;

                if (transform.ScaleZ < 0)
                {
                    float temp = depthNear;
                    depthNear = depthFar;
                    depthFar = temp;
                }

                viewports[index] = new Viewport(region, swizzleX, swizzleY, swizzleZ, swizzleW, depthNear, depthFar);
            }

            _context.Renderer.Pipeline.SetViewports(0, viewports);
        }

        /// <summary>
        /// Updates the depth mode (0 to 1 or -1 to 1) based on the current viewport and depth mode register state.
        /// </summary>
        private void UpdateDepthMode()
        {
            ref var transform = ref _state.State.ViewportTransform[0];
            ref var extents = ref _state.State.ViewportExtents[0];

            DepthMode depthMode;

            if (!float.IsInfinity(extents.DepthNear) &&
                !float.IsInfinity(extents.DepthFar) &&
                (extents.DepthFar - extents.DepthNear) != 0)
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
                depthMode = extents.DepthNear != transform.TranslateZ &&
                            extents.DepthFar  != transform.TranslateZ
                    ? DepthMode.MinusOneToOne
                    : DepthMode.ZeroToOne;
            }
            else
            {
                // If we can't guess from the viewport transform, then just use the depth mode register.
                depthMode = (DepthMode)(_state.State.DepthMode & 1);
            }

            _context.Renderer.Pipeline.SetDepthMode(depthMode);
        }

        /// <summary>
        /// Updates polygon mode state based on current GPU state.
        /// </summary>
        private void UpdatePolygonMode()
        {
            _context.Renderer.Pipeline.SetPolygonMode(_state.State.PolygonModeFront, _state.State.PolygonModeBack);
        }

        /// <summary>
        /// Updates host depth bias (also called polygon offset) state based on current GPU state.
        /// </summary>
        private void UpdateDepthBiasState()
        {
            var depthBias = _state.State.DepthBiasState;

            float factor = _state.State.DepthBiasFactor;
            float units = _state.State.DepthBiasUnits;
            float clamp = _state.State.DepthBiasClamp;

            PolygonModeMask enables;

            enables = (depthBias.PointEnable ? PolygonModeMask.Point : 0);
            enables |= (depthBias.LineEnable ? PolygonModeMask.Line : 0);
            enables |= (depthBias.FillEnable ? PolygonModeMask.Fill : 0);

            _context.Renderer.Pipeline.SetDepthBias(enables, factor, units / 2f, clamp);
        }

        /// <summary>
        /// Updates host stencil test state based on current GPU state.
        /// </summary>
        private void UpdateStencilTestState()
        {
            var backMasks = _state.State.StencilBackMasks;
            var test = _state.State.StencilTestState;
            var backTest = _state.State.StencilBackTestState;

            CompareOp backFunc;
            StencilOp backSFail;
            StencilOp backDpPass;
            StencilOp backDpFail;
            int backFuncRef;
            int backFuncMask;
            int backMask;

            if (backTest.TwoSided)
            {
                backFunc = backTest.BackFunc;
                backSFail = backTest.BackSFail;
                backDpPass = backTest.BackDpPass;
                backDpFail = backTest.BackDpFail;
                backFuncRef = backMasks.FuncRef;
                backFuncMask = backMasks.FuncMask;
                backMask = backMasks.Mask;
            }
            else
            {
                backFunc = test.FrontFunc;
                backSFail = test.FrontSFail;
                backDpPass = test.FrontDpPass;
                backDpFail = test.FrontDpFail;
                backFuncRef = test.FrontFuncRef;
                backFuncMask = test.FrontFuncMask;
                backMask = test.FrontMask;
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
        /// Updates user-defined clipping based on the guest GPU state.
        /// </summary>
        private void UpdateUserClipState()
        {
            uint clipMask = _state.State.ClipDistanceEnable & _vsClipDistancesWritten;

            for (int i = 0; i < Constants.TotalClipDistances; ++i)
            {
                _context.Renderer.Pipeline.SetUserClipDistance(i, (clipMask & (1 << i)) != 0);
            }
        }

        /// <summary>
        /// Updates current sampler pool address and size based on guest GPU state.
        /// </summary>
        private void UpdateSamplerPoolState()
        {
            var texturePool = _state.State.TexturePoolState;
            var samplerPool = _state.State.SamplerPoolState;

            var samplerIndex = _state.State.SamplerIndex;

            int maximumId = samplerIndex == SamplerIndex.ViaHeaderIndex
                ? texturePool.MaximumId
                : samplerPool.MaximumId;

            _channel.TextureManager.SetGraphicsSamplerPool(samplerPool.Address.Pack(), maximumId, samplerIndex);
        }

        /// <summary>
        /// Updates current texture pool address and size based on guest GPU state.
        /// </summary>
        private void UpdateTexturePoolState()
        {
            var texturePool = _state.State.TexturePoolState;

            _channel.TextureManager.SetGraphicsTexturePool(texturePool.Address.Pack(), texturePool.MaximumId);
            _channel.TextureManager.SetGraphicsTextureBufferIndex((int)_state.State.TextureBufferIndex);
        }

        /// <summary>
        /// Updates host vertex attributes based on guest GPU state.
        /// </summary>
        private void UpdateVertexAttribState()
        {
            Span<VertexAttribDescriptor> vertexAttribs = stackalloc VertexAttribDescriptor[Constants.TotalVertexAttribs];

            for (int index = 0; index < Constants.TotalVertexAttribs; index++)
            {
                var vertexAttrib = _state.State.VertexAttribState[index];

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
        /// Updates host line width based on guest GPU state.
        /// </summary>
        private void UpdateLineState()
        {
            float width = _state.State.LineWidthSmooth;
            bool smooth = _state.State.LineSmoothEnable;

            _context.Renderer.Pipeline.SetLineParameters(width, smooth);
        }

        /// <summary>
        /// Updates host point size based on guest GPU state.
        /// </summary>
        private void UpdatePointState()
        {
            float size = _state.State.PointSize;
            bool isProgramPointSize = _state.State.VertexProgramPointSize;
            bool enablePointSprite = _state.State.PointSpriteEnable;

            // TODO: Need to figure out a way to map PointCoordReplace enable bit.
            Origin origin = (_state.State.PointCoordReplace & 4) == 0 ? Origin.LowerLeft : Origin.UpperLeft;

            _context.Renderer.Pipeline.SetPointParameters(size, isProgramPointSize, enablePointSprite, origin);
        }

        /// <summary>
        /// Updates host primitive restart based on guest GPU state.
        /// </summary>
        private void UpdatePrimitiveRestartState()
        {
            PrimitiveRestartState primitiveRestart = _state.State.PrimitiveRestartState;
            bool enable = primitiveRestart.Enable && (_drawState.DrawIndexed || _state.State.PrimitiveRestartDrawArrays);

            _context.Renderer.Pipeline.SetPrimitiveRestart(enable, primitiveRestart.Index);
        }

        /// <summary>
        /// Updates host index buffer binding based on guest GPU state.
        /// </summary>
        private void UpdateIndexBufferState()
        {
            var indexBuffer = _state.State.IndexBufferState;

            if (_drawState.IndexCount == 0)
            {
                return;
            }

            ulong gpuVa = indexBuffer.Address.Pack();

            // Do not use the end address to calculate the size, because
            // the result may be much larger than the real size of the index buffer.
            ulong size = (ulong)(_drawState.FirstIndex + _drawState.IndexCount);

            switch (indexBuffer.Type)
            {
                case IndexType.UShort: size *= 2; break;
                case IndexType.UInt: size *= 4; break;
            }

            _channel.BufferManager.SetIndexBuffer(gpuVa, size, indexBuffer.Type);
        }

        /// <summary>
        /// Updates host vertex buffer bindings based on guest GPU state.
        /// </summary>
        private void UpdateVertexBufferState()
        {
            IndexType indexType = _state.State.IndexBufferState.Type;
            bool indexTypeSmall = indexType == IndexType.UByte || indexType == IndexType.UShort;

            _drawState.IsAnyVbInstanced = false;

            for (int index = 0; index < Constants.TotalVertexBuffers; index++)
            {
                var vertexBuffer = _state.State.VertexBufferState[index];

                if (!vertexBuffer.UnpackEnable())
                {
                    _channel.BufferManager.SetVertexBuffer(index, 0, 0, 0, 0);

                    continue;
                }

                GpuVa endAddress = _state.State.VertexBufferEndAddress[index];

                ulong address = vertexBuffer.Address.Pack();

                int stride = vertexBuffer.UnpackStride();

                bool instanced = _state.State.VertexBufferInstanced[index];

                int divisor = instanced ? vertexBuffer.Divisor : 0;

                _drawState.IsAnyVbInstanced |= divisor != 0;

                ulong size;

                if (_drawState.IbStreamer.HasInlineIndexData || _drawState.DrawIndexed || stride == 0 || instanced)
                {
                    // This size may be (much) larger than the real vertex buffer size.
                    // Avoid calculating it this way, unless we don't have any other option.

                    size = endAddress.Pack() - address + 1;

                    if (stride > 0 && indexTypeSmall && _drawState.DrawIndexed && !instanced)
                    {
                        // If the index type is a small integer type, then we might be still able
                        // to reduce the vertex buffer size based on the maximum possible index value.

                        ulong maxVertexBufferSize = indexType == IndexType.UByte ? 0x100UL : 0x10000UL;

                        maxVertexBufferSize += _state.State.FirstVertex;
                        maxVertexBufferSize *= (uint)stride;

                        size = Math.Min(size, maxVertexBufferSize);
                    }
                }
                else
                {
                    // For non-indexed draws, we can guess the size from the vertex count
                    // and stride.

                    int firstInstance = (int)_state.State.FirstInstance;

                    var drawState = _state.State.VertexBufferDrawState;

                    size = (ulong)((firstInstance + drawState.First + drawState.Count) * stride);
                }

                _channel.BufferManager.SetVertexBuffer(index, address, size, stride, divisor);
            }
        }

        /// <summary>
        /// Updates host face culling and orientation based on guest GPU state.
        /// </summary>
        private void UpdateFaceState()
        {
            var yControl = _state.State.YControl;
            var face = _state.State.FaceState;

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
        private void UpdateRtColorMask()
        {
            bool rtColorMaskShared = _state.State.RtColorMaskShared;

            Span<uint> componentMasks = stackalloc uint[Constants.TotalRenderTargets];

            for (int index = 0; index < Constants.TotalRenderTargets; index++)
            {
                var colorMask = _state.State.RtColorMask[rtColorMaskShared ? 0 : index];

                uint componentMask;

                componentMask = (colorMask.UnpackRed() ? 1u : 0u);
                componentMask |= (colorMask.UnpackGreen() ? 2u : 0u);
                componentMask |= (colorMask.UnpackBlue() ? 4u : 0u);
                componentMask |= (colorMask.UnpackAlpha() ? 8u : 0u);

                componentMasks[index] = componentMask;
            }

            _context.Renderer.Pipeline.SetRenderTargetColorMasks(componentMasks);
        }

        /// <summary>
        /// Updates host render target color buffer blending state, based on guest state.
        /// </summary>
        private void UpdateBlendState()
        {
            bool blendIndependent = _state.State.BlendIndependent;
            ColorF blendConstant = _state.State.BlendConstant;

            for (int index = 0; index < Constants.TotalRenderTargets; index++)
            {
                BlendDescriptor descriptor;

                if (blendIndependent)
                {
                    bool enable = _state.State.BlendEnable[index];
                    var blend = _state.State.BlendState[index];

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
                    bool enable = _state.State.BlendEnable[0];
                    var blend = _state.State.BlendStateCommon;

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
        private void UpdateLogicOpState()
        {
            LogicalOpState logicOpState = _state.State.LogicOpState;

            _context.Renderer.Pipeline.SetLogicOpState(logicOpState.Enable, logicOpState.LogicalOp);
        }

        /// <summary>
        /// Updates host shaders based on the guest GPU state.
        /// </summary>
        private void UpdateShaderState()
        {
            var shaderCache = _channel.MemoryManager.Physical.ShaderCache;

            _vtgWritesRtLayer = false;

            ShaderAddresses addresses = new ShaderAddresses();
            Span<ulong> addressesSpan = addresses.AsSpan();

            ulong baseAddress = _state.State.ShaderBaseAddress.Pack();

            for (int index = 0; index < 6; index++)
            {
                var shader = _state.State.ShaderState[index];
                if (!shader.UnpackEnable() && index != 1)
                {
                    continue;
                }

                addressesSpan[index] = baseAddress + shader.Offset;
            }

            GpuChannelPoolState poolState = GetPoolState();
            GpuChannelGraphicsState graphicsState = GetGraphicsState();

            CachedShaderProgram gs = shaderCache.GetGraphicsShader(ref _state.State, _channel, poolState, graphicsState, addresses);

            _shaderSpecState = gs.SpecializationState;

            byte oldVsClipDistancesWritten = _vsClipDistancesWritten;

            _drawState.VsUsesInstanceId = gs.Shaders[1]?.Info.UsesInstanceId ?? false;
            _vsClipDistancesWritten = gs.Shaders[1]?.Info.ClipDistancesWritten ?? 0;

            if (oldVsClipDistancesWritten != _vsClipDistancesWritten)
            {
                UpdateUserClipState();
            }

            for (int stageIndex = 0; stageIndex < Constants.ShaderStages; stageIndex++)
            {
                UpdateStageBindings(stageIndex, gs.Shaders[stageIndex + 1]?.Info);
            }

            _context.Renderer.Pipeline.SetProgram(gs.HostProgram);
        }

        private void UpdateStageBindings(int stage, ShaderProgramInfo info)
        {
            _currentProgramInfo[stage] = info;

            if (info == null)
            {
                _channel.TextureManager.RentGraphicsTextureBindings(stage, 0);
                _channel.TextureManager.RentGraphicsImageBindings(stage, 0);
                _channel.BufferManager.SetGraphicsStorageBufferBindings(stage, null);
                _channel.BufferManager.SetGraphicsUniformBufferBindings(stage, null);
                return;
            }

            Span<TextureBindingInfo> textureBindings = _channel.TextureManager.RentGraphicsTextureBindings(stage, info.Textures.Count);

            if (info.UsesRtLayer)
            {
                _vtgWritesRtLayer = true;
            }

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

            TextureBindingInfo[] imageBindings = _channel.TextureManager.RentGraphicsImageBindings(stage, info.Images.Count);

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

            _channel.BufferManager.SetGraphicsStorageBufferBindings(stage, info.SBuffers);
            _channel.BufferManager.SetGraphicsUniformBufferBindings(stage, info.CBuffers);
        }

        private GpuChannelPoolState GetPoolState()
        {
            return new GpuChannelPoolState(
                _state.State.TexturePoolState.Address.Pack(),
                _state.State.TexturePoolState.MaximumId,
                (int)_state.State.TextureBufferIndex);
        }

        /// <summary>
        /// Gets the current GPU channel state for shader creation or compatibility verification.
        /// </summary>
        /// <returns>Current GPU channel state</returns>
        private GpuChannelGraphicsState GetGraphicsState()
        {
            return new GpuChannelGraphicsState(
                _state.State.EarlyZForce,
                _drawState.Topology,
                _state.State.TessMode);
        }

        /// <summary>
        /// Forces the shaders to be rebound on the next draw.
        /// </summary>
        public void ForceShaderUpdate()
        {
            _updateTracker.ForceDirty(ShaderStateIndex);
        }
    }
}

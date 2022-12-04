using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.Types;
using Ryujinx.Graphics.Gpu.Memory;
using System;

namespace Ryujinx.Graphics.Gpu.Engine.Threed
{
    /// <summary>
    /// Draw manager.
    /// </summary>
    class DrawManager
    {
        // Since we don't know the index buffer size for indirect draws,
        // we must assume a minimum and maximum size and use that for buffer data update purposes.
        private const int MinIndirectIndexCount = 0x10000;
        private const int MaxIndirectIndexCount = 0x4000000;

        private readonly GpuContext _context;
        private readonly GpuChannel _channel;
        private readonly DeviceStateWithShadow<ThreedClassState> _state;
        private readonly DrawState _drawState;
        private readonly SpecializationStateUpdater _currentSpecState;
        private bool _topologySet;

        private bool _instancedDrawPending;
        private bool _instancedIndexed;
        private bool _instancedIndexedInline;

        private int _instancedFirstIndex;
        private int _instancedFirstVertex;
        private int _instancedFirstInstance;
        private int _instancedIndexCount;
        private int _instancedDrawStateFirst;
        private int _instancedDrawStateCount;

        private int _instanceIndex;

        private const int VertexBufferFirstMethodOffset = 0x35d;
        private const int IndexBufferCountMethodOffset = 0x5f8;

        /// <summary>
        /// Creates a new instance of the draw manager.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="channel">GPU channel</param>
        /// <param name="state">Channel state</param>
        /// <param name="drawState">Draw state</param>
        /// <param name="spec">Specialization state updater</param>
        public DrawManager(GpuContext context, GpuChannel channel, DeviceStateWithShadow<ThreedClassState> state, DrawState drawState, SpecializationStateUpdater spec)
        {
            _context = context;
            _channel = channel;
            _state = state;
            _drawState = drawState;
            _currentSpecState = spec;
        }

        /// <summary>
        /// Marks the entire state as dirty, forcing a full host state update before the next draw.
        /// </summary>
        public void ForceStateDirty()
        {
            _topologySet = false;
        }

        /// <summary>
        /// Pushes four 8-bit index buffer elements.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        public void VbElementU8(int argument)
        {
            _drawState.IbStreamer.VbElementU8(_context.Renderer, argument);
        }

        /// <summary>
        /// Pushes two 16-bit index buffer elements.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        public void VbElementU16(int argument)
        {
            _drawState.IbStreamer.VbElementU16(_context.Renderer, argument);
        }

        /// <summary>
        /// Pushes one 32-bit index buffer element.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        public void VbElementU32(int argument)
        {
            _drawState.IbStreamer.VbElementU32(_context.Renderer, argument);
        }

        /// <summary>
        /// Finishes the draw call.
        /// This draws geometry on the bound buffers based on the current GPU state.
        /// </summary>
        /// <param name="engine">3D engine where this method is being called</param>
        /// <param name="argument">Method call argument</param>
        public void DrawEnd(ThreedClass engine, int argument)
        {
            DrawEnd(engine, _state.State.IndexBufferState.First, (int)_state.State.IndexBufferCount);
        }

        /// <summary>
        /// Finishes the draw call.
        /// This draws geometry on the bound buffers based on the current GPU state.
        /// </summary>
        /// <param name="engine">3D engine where this method is being called</param>
        /// <param name="firstIndex">Index of the first index buffer element used on the draw</param>
        /// <param name="indexCount">Number of index buffer elements used on the draw</param>
        private void DrawEnd(ThreedClass engine, int firstIndex, int indexCount)
        {
            ConditionalRenderEnabled renderEnable = ConditionalRendering.GetRenderEnable(
                _context,
                _channel.MemoryManager,
                _state.State.RenderEnableAddress,
                _state.State.RenderEnableCondition);

            if (renderEnable == ConditionalRenderEnabled.False || _instancedDrawPending)
            {
                if (renderEnable == ConditionalRenderEnabled.False)
                {
                    PerformDeferredDraws();
                }

                _drawState.DrawIndexed = false;

                if (renderEnable == ConditionalRenderEnabled.Host)
                {
                    _context.Renderer.Pipeline.EndHostConditionalRendering();
                }

                return;
            }

            _drawState.FirstIndex = firstIndex;
            _drawState.IndexCount = indexCount;
            _currentSpecState.SetHasConstantBufferDrawParameters(false);

            engine.UpdateState();

            bool instanced = _drawState.VsUsesInstanceId || _drawState.IsAnyVbInstanced;

            if (instanced)
            {
                _instancedDrawPending = true;

                int ibCount = _drawState.IbStreamer.InlineIndexCount;

                _instancedIndexed = _drawState.DrawIndexed;
                _instancedIndexedInline = ibCount != 0;

                _instancedFirstIndex = firstIndex;
                _instancedFirstVertex = (int)_state.State.FirstVertex;
                _instancedFirstInstance = (int)_state.State.FirstInstance;

                _instancedIndexCount = ibCount != 0 ? ibCount : indexCount;

                var drawState = _state.State.VertexBufferDrawState;

                _instancedDrawStateFirst = drawState.First;
                _instancedDrawStateCount = drawState.Count;

                _drawState.DrawIndexed = false;

                if (renderEnable == ConditionalRenderEnabled.Host)
                {
                    _context.Renderer.Pipeline.EndHostConditionalRendering();
                }

                return;
            }

            int firstInstance = (int)_state.State.FirstInstance;

            int inlineIndexCount = _drawState.IbStreamer.GetAndResetInlineIndexCount();

            if (inlineIndexCount != 0)
            {
                int firstVertex = (int)_state.State.FirstVertex;

                BufferRange br = new BufferRange(_drawState.IbStreamer.GetInlineIndexBuffer(), 0, inlineIndexCount * 4);

                _channel.BufferManager.SetIndexBuffer(br, IndexType.UInt);

                _context.Renderer.Pipeline.DrawIndexed(inlineIndexCount, 1, firstIndex, firstVertex, firstInstance);
            }
            else if (_drawState.DrawIndexed)
            {
                int firstVertex = (int)_state.State.FirstVertex;

                _context.Renderer.Pipeline.DrawIndexed(indexCount, 1, firstIndex, firstVertex, firstInstance);
            }
            else
            {
                var drawState = _state.State.VertexBufferDrawState;

                _context.Renderer.Pipeline.Draw(drawState.Count, 1, drawState.First, firstInstance);
            }

            _drawState.DrawIndexed = false;

            if (renderEnable == ConditionalRenderEnabled.Host)
            {
                _context.Renderer.Pipeline.EndHostConditionalRendering();
            }
        }

        /// <summary>
        /// Starts draw.
        /// This sets primitive type and instanced draw parameters.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        public void DrawBegin(int argument)
        {
            bool incrementInstance = (argument & (1 << 26)) != 0;
            bool resetInstance = (argument & (1 << 27)) == 0;

            if (_state.State.PrimitiveTypeOverrideEnable)
            {
                PrimitiveTypeOverride typeOverride = _state.State.PrimitiveTypeOverride;
                DrawBegin(incrementInstance, resetInstance, typeOverride.Convert());
            }
            else
            {
                PrimitiveType type = (PrimitiveType)(argument & 0xffff);
                DrawBegin(incrementInstance, resetInstance, type.Convert());
            }
        }

        /// <summary>
        /// Starts draw.
        /// This sets primitive type and instanced draw parameters.
        /// </summary>
        /// <param name="incrementInstance">Indicates if the current instance should be incremented</param>
        /// <param name="resetInstance">Indicates if the current instance should be set to zero</param>
        /// <param name="topology">Primitive topology</param>
        private void DrawBegin(bool incrementInstance, bool resetInstance, PrimitiveTopology topology)
        {
            if (incrementInstance)
            {
                _instanceIndex++;
            }
            else if (resetInstance)
            {
                PerformDeferredDraws();

                _instanceIndex = 0;
            }

            UpdateTopology(topology);
        }

        /// <summary>
        /// Updates the current primitive topology if needed.
        /// </summary>
        /// <param name="topology">New primitive topology</param>
        private void UpdateTopology(PrimitiveTopology topology)
        {
            if (_drawState.Topology != topology || !_topologySet)
            {
                _context.Renderer.Pipeline.SetPrimitiveTopology(topology);
                _currentSpecState.SetTopology(topology);
                _drawState.Topology = topology;
                _topologySet = true;
            }
        }

        /// <summary>
        /// Sets the index buffer count.
        /// This also sets internal state that indicates that the next draw is an indexed draw.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        public void SetIndexBufferCount(int argument)
        {
            _drawState.DrawIndexed = true;
        }

        /// <summary>
        /// Performs a indexed draw with a low number of index buffer elements.
        /// </summary>
        /// <param name="engine">3D engine where this method is being called</param>
        /// <param name="argument">Method call argument</param>
        public void DrawIndexedSmall(ThreedClass engine, int argument)
        {
            DrawIndexedSmall(engine, argument, false);
        }

        /// <summary>
        /// Performs a indexed draw with a low number of index buffer elements.
        /// </summary>
        /// <param name="engine">3D engine where this method is being called</param>
        /// <param name="argument">Method call argument</param>
        public void DrawIndexedSmall2(ThreedClass engine, int argument)
        {
            DrawIndexedSmall(engine, argument);
        }

        /// <summary>
        /// Performs a indexed draw with a low number of index buffer elements,
        /// while also pre-incrementing the current instance value.
        /// </summary>
        /// <param name="engine">3D engine where this method is being called</param>
        /// <param name="argument">Method call argument</param>
        public void DrawIndexedSmallIncInstance(ThreedClass engine, int argument)
        {
            DrawIndexedSmall(engine, argument, true);
        }

        /// <summary>
        /// Performs a indexed draw with a low number of index buffer elements,
        /// while also pre-incrementing the current instance value.
        /// </summary>
        /// <param name="engine">3D engine where this method is being called</param>
        /// <param name="argument">Method call argument</param>
        public void DrawIndexedSmallIncInstance2(ThreedClass engine, int argument)
        {
            DrawIndexedSmallIncInstance(engine, argument);
        }

        /// <summary>
        /// Performs a indexed draw with a low number of index buffer elements,
        /// while optionally also pre-incrementing the current instance value.
        /// </summary>
        /// <param name="engine">3D engine where this method is being called</param>
        /// <param name="argument">Method call argument</param>
        /// <param name="instanced">True to increment the current instance value, false otherwise</param>
        private void DrawIndexedSmall(ThreedClass engine, int argument, bool instanced)
        {
            PrimitiveTypeOverride typeOverride = _state.State.PrimitiveTypeOverride;

            DrawBegin(instanced, !instanced, typeOverride.Convert());

            int firstIndex = argument & 0xffff;
            int indexCount = (argument >> 16) & 0xfff;

            bool oldDrawIndexed = _drawState.DrawIndexed;

            _drawState.DrawIndexed = true;
            engine.ForceStateDirty(IndexBufferCountMethodOffset * 4);

            DrawEnd(engine, firstIndex, indexCount);

            _drawState.DrawIndexed = oldDrawIndexed;
        }

        /// <summary>
        /// Performs a texture draw with a source texture and sampler ID, along with source
        /// and destination coordinates and sizes.
        /// </summary>
        /// <param name="engine">3D engine where this method is being called</param>
        /// <param name="argument">Method call argument</param>
        public void DrawTexture(ThreedClass engine, int argument)
        {
            static float FixedToFloat(int fixedValue)
            {
                return fixedValue * (1f / 4096);
            }

            float dstX0 = FixedToFloat(_state.State.DrawTextureDstX);
            float dstY0 = FixedToFloat(_state.State.DrawTextureDstY);
            float dstWidth = FixedToFloat(_state.State.DrawTextureDstWidth);
            float dstHeight = FixedToFloat(_state.State.DrawTextureDstHeight);

            // TODO: Confirm behaviour on hardware.
            // When this is active, the origin appears to be on the bottom.
            if (_state.State.YControl.HasFlag(YControl.NegateY))
            {
                dstY0 -= dstHeight;
            }

            float dstX1 = dstX0 + dstWidth;
            float dstY1 = dstY0 + dstHeight;

            float srcX0 = FixedToFloat(_state.State.DrawTextureSrcX);
            float srcY0 = FixedToFloat(_state.State.DrawTextureSrcY);
            float srcX1 = ((float)_state.State.DrawTextureDuDx / (1UL << 32)) * dstWidth + srcX0;
            float srcY1 = ((float)_state.State.DrawTextureDvDy / (1UL << 32)) * dstHeight + srcY0;

            engine.UpdateState(ulong.MaxValue & ~(1UL << StateUpdater.ShaderStateIndex));

            _channel.TextureManager.UpdateRenderTargets();

            int textureId = _state.State.DrawTextureTextureId;
            int samplerId = _state.State.DrawTextureSamplerId;

            (var texture, var sampler) = _channel.TextureManager.GetGraphicsTextureAndSampler(textureId, samplerId);

            srcX0 *= texture.ScaleFactor;
            srcY0 *= texture.ScaleFactor;
            srcX1 *= texture.ScaleFactor;
            srcY1 *= texture.ScaleFactor;

            float dstScale = _channel.TextureManager.RenderTargetScale;

            dstX0 *= dstScale;
            dstY0 *= dstScale;
            dstX1 *= dstScale;
            dstY1 *= dstScale;

            _context.Renderer.Pipeline.DrawTexture(
                texture?.HostTexture,
                sampler?.GetHostSampler(texture),
                new Extents2DF(srcX0, srcY0, srcX1, srcY1),
                new Extents2DF(dstX0, dstY0, dstX1, dstY1));
        }

        /// <summary>
        /// Performs a indexed or non-indexed draw.
        /// </summary>
        /// <param name="engine">3D engine where this method is being called</param>
        /// <param name="topology">Primitive topology</param>
        /// <param name="count">Index count for indexed draws, vertex count for non-indexed draws</param>
        /// <param name="instanceCount">Instance count</param>
        /// <param name="firstIndex">First index on the index buffer for indexed draws, ignored for non-indexed draws</param>
        /// <param name="firstVertex">First vertex on the vertex buffer</param>
        /// <param name="firstInstance">First instance</param>
        /// <param name="indexed">True if the draw is indexed, false otherwise</param>
        public void Draw(
            ThreedClass engine,
            PrimitiveTopology topology,
            int count,
            int instanceCount,
            int firstIndex,
            int firstVertex,
            int firstInstance,
            bool indexed)
        {
            UpdateTopology(topology);

            ConditionalRenderEnabled renderEnable = ConditionalRendering.GetRenderEnable(
                _context,
                _channel.MemoryManager,
                _state.State.RenderEnableAddress,
                _state.State.RenderEnableCondition);

            if (renderEnable == ConditionalRenderEnabled.False)
            {
                _drawState.DrawIndexed = false;
                return;
            }

            if (indexed)
            {
                _drawState.FirstIndex = firstIndex;
                _drawState.IndexCount = count;
                _state.State.FirstVertex = (uint)firstVertex;
                engine.ForceStateDirty(IndexBufferCountMethodOffset * 4);
            }
            else
            {
                _state.State.VertexBufferDrawState.First = firstVertex;
                _state.State.VertexBufferDrawState.Count = count;
                engine.ForceStateDirty(VertexBufferFirstMethodOffset * 4);
            }

            _state.State.FirstInstance = (uint)firstInstance;

            _drawState.DrawIndexed = indexed;
            _currentSpecState.SetHasConstantBufferDrawParameters(true);

            engine.UpdateState();

            if (indexed)
            {
                _context.Renderer.Pipeline.DrawIndexed(count, instanceCount, firstIndex, firstVertex, firstInstance);
                _state.State.FirstVertex = 0;
            }
            else
            {
                _context.Renderer.Pipeline.Draw(count, instanceCount, firstVertex, firstInstance);
            }

            _state.State.FirstInstance = 0;

            _drawState.DrawIndexed = false;

            if (renderEnable == ConditionalRenderEnabled.Host)
            {
                _context.Renderer.Pipeline.EndHostConditionalRendering();
            }
        }

        /// <summary>
        /// Performs a indirect draw, with parameters from a GPU buffer.
        /// </summary>
        /// <param name="engine">3D engine where this method is being called</param>
        /// <param name="topology">Primitive topology</param>
        /// <param name="indirectBufferAddress">Address of the buffer with the draw parameters, such as count, first index, etc</param>
        /// <param name="parameterBufferAddress">Address of the buffer with the draw count</param>
        /// <param name="maxDrawCount">Maximum number of draws that can be made</param>
        /// <param name="stride">Distance in bytes between each entry on the data pointed to by <paramref name="indirectBufferAddress"/></param>
        /// <param name="indexCount">Maximum number of indices that the draw can consume</param>
        /// <param name="drawType">Type of the indirect draw, which can be indexed or non-indexed, with or without a draw count</param>
        public void DrawIndirect(
            ThreedClass engine,
            PrimitiveTopology topology,
            ulong indirectBufferAddress,
            ulong parameterBufferAddress,
            int maxDrawCount,
            int stride,
            int indexCount,
            IndirectDrawType drawType)
        {
            UpdateTopology(topology);

            ConditionalRenderEnabled renderEnable = ConditionalRendering.GetRenderEnable(
                _context,
                _channel.MemoryManager,
                _state.State.RenderEnableAddress,
                _state.State.RenderEnableCondition);

            if (renderEnable == ConditionalRenderEnabled.False)
            {
                _drawState.DrawIndexed = false;
                return;
            }

            PhysicalMemory memory = _channel.MemoryManager.Physical;

            bool hasCount = (drawType & IndirectDrawType.Count) != 0;
            bool indexed = (drawType & IndirectDrawType.Indexed) != 0;

            if (indexed)
            {
                indexCount = Math.Clamp(indexCount, MinIndirectIndexCount, MaxIndirectIndexCount);
                _drawState.FirstIndex = 0;
                _drawState.IndexCount = indexCount;
                engine.ForceStateDirty(IndexBufferCountMethodOffset * 4);
            }

            _drawState.DrawIndexed = indexed;
            _drawState.DrawIndirect = true;
            _currentSpecState.SetHasConstantBufferDrawParameters(true);

            engine.UpdateState();

            if (hasCount)
            {
                var indirectBuffer = memory.BufferCache.GetBufferRange(indirectBufferAddress, (ulong)maxDrawCount * (ulong)stride);
                var parameterBuffer = memory.BufferCache.GetBufferRange(parameterBufferAddress, 4);

                if (indexed)
                {
                    _context.Renderer.Pipeline.DrawIndexedIndirectCount(indirectBuffer, parameterBuffer, maxDrawCount, stride);
                }
                else
                {
                    _context.Renderer.Pipeline.DrawIndirectCount(indirectBuffer, parameterBuffer, maxDrawCount, stride);
                }
            }
            else
            {
                var indirectBuffer = memory.BufferCache.GetBufferRange(indirectBufferAddress, (ulong)stride);

                if (indexed)
                {
                    _context.Renderer.Pipeline.DrawIndexedIndirect(indirectBuffer);
                }
                else
                {
                    _context.Renderer.Pipeline.DrawIndirect(indirectBuffer);
                }
            }

            _drawState.DrawIndexed = false;
            _drawState.DrawIndirect = false;

            if (renderEnable == ConditionalRenderEnabled.Host)
            {
                _context.Renderer.Pipeline.EndHostConditionalRendering();
            }
        }

        /// <summary>
        /// Perform any deferred draws.
        /// This is used for instanced draws.
        /// Since each instance is a separate draw, we defer the draw and accumulate the instance count.
        /// Once we detect the last instanced draw, then we perform the host instanced draw,
        /// with the accumulated instance count.
        /// </summary>
        public void PerformDeferredDraws()
        {
            // Perform any pending instanced draw.
            if (_instancedDrawPending)
            {
                _instancedDrawPending = false;

                bool indexedInline = _instancedIndexedInline;

                if (_instancedIndexed || indexedInline)
                {
                    if (indexedInline)
                    {
                        int inlineIndexCount = _drawState.IbStreamer.GetAndResetInlineIndexCount();
                        BufferRange br = new BufferRange(_drawState.IbStreamer.GetInlineIndexBuffer(), 0, inlineIndexCount * 4);

                        _channel.BufferManager.SetIndexBuffer(br, IndexType.UInt);
                    }

                    _context.Renderer.Pipeline.DrawIndexed(
                        _instancedIndexCount,
                        _instanceIndex + 1,
                        _instancedFirstIndex,
                        _instancedFirstVertex,
                        _instancedFirstInstance);
                }
                else
                {
                    _context.Renderer.Pipeline.Draw(
                        _instancedDrawStateCount,
                        _instanceIndex + 1,
                        _instancedDrawStateFirst,
                        _instancedFirstInstance);
                }
            }
        }

        /// <summary>
        /// Clears the current color and depth-stencil buffers.
        /// Which buffers should be cleared can also be specified with the argument.
        /// </summary>
        /// <param name="engine">3D engine where this method is being called</param>
        /// <param name="argument">Method call argument</param>
        public void Clear(ThreedClass engine, int argument)
        {
            Clear(engine, argument, 1);
        }

        /// <summary>
        /// Clears the current color and depth-stencil buffers.
        /// Which buffers should be cleared can also specified with the arguments.
        /// </summary>
        /// <param name="engine">3D engine where this method is being called</param>
        /// <param name="argument">Method call argument</param>
        /// <param name="layerCount">For array and 3D textures, indicates how many layers should be cleared</param>
        public void Clear(ThreedClass engine, int argument, int layerCount)
        {
            ConditionalRenderEnabled renderEnable = ConditionalRendering.GetRenderEnable(
                _context,
                _channel.MemoryManager,
                _state.State.RenderEnableAddress,
                _state.State.RenderEnableCondition);

            if (renderEnable == ConditionalRenderEnabled.False)
            {
                return;
            }

            int index = (argument >> 6) & 0xf;
            int layer = (argument >> 10) & 0x3ff;

            engine.UpdateRenderTargetState(useControl: false, layered: layer != 0 || layerCount > 1, singleUse: index);

            // If there is a mismatch on the host clip region and the one explicitly defined by the guest
            // on the screen scissor state, then we need to force only one texture to be bound to avoid
            // host clipping.
            var screenScissorState = _state.State.ScreenScissorState;

            // Must happen after UpdateRenderTargetState to have up-to-date clip region values.
            bool clipMismatch = (screenScissorState.X | screenScissorState.Y) != 0 ||
                                screenScissorState.Width != _channel.TextureManager.ClipRegionWidth ||
                                screenScissorState.Height != _channel.TextureManager.ClipRegionHeight;

            bool clearAffectedByStencilMask = (_state.State.ClearFlags & 1) != 0;
            bool clearAffectedByScissor = (_state.State.ClearFlags & 0x100) != 0;
            bool needsCustomScissor = !clearAffectedByScissor || clipMismatch;

            // Scissor and rasterizer discard also affect clears.
            ulong updateMask = 1UL << StateUpdater.RasterizerStateIndex;

            if (!needsCustomScissor)
            {
                updateMask |= 1UL << StateUpdater.ScissorStateIndex;
            }

            engine.UpdateState(updateMask);

            if (needsCustomScissor)
            {
                int scissorX = screenScissorState.X;
                int scissorY = screenScissorState.Y;
                int scissorW = screenScissorState.Width;
                int scissorH = screenScissorState.Height;

                if (clearAffectedByScissor && _state.State.ScissorState[0].Enable)
                {
                    ref var scissorState = ref _state.State.ScissorState[0];

                    scissorX = Math.Max(scissorX, scissorState.X1);
                    scissorY = Math.Max(scissorY, scissorState.Y1);
                    scissorW = Math.Min(scissorW, scissorState.X2 - scissorState.X1);
                    scissorH = Math.Min(scissorH, scissorState.Y2 - scissorState.Y1);
                }

                float scale = _channel.TextureManager.RenderTargetScale;
                if (scale != 1f)
                {
                    scissorX = (int)(scissorX * scale);
                    scissorY = (int)(scissorY * scale);
                    scissorW = (int)MathF.Ceiling(scissorW * scale);
                    scissorH = (int)MathF.Ceiling(scissorH * scale);
                }

                Span<Rectangle<int>> scissors = stackalloc Rectangle<int>[]
                {
                    new Rectangle<int>(scissorX, scissorY, scissorW, scissorH)
                };

                _context.Renderer.Pipeline.SetScissors(scissors);
            }

            if (clipMismatch)
            {
                _channel.TextureManager.UpdateRenderTarget(index);
            }
            else
            {
                _channel.TextureManager.UpdateRenderTargets();
            }

            bool clearDepth = (argument & 1) != 0;
            bool clearStencil = (argument & 2) != 0;
            uint componentMask = (uint)((argument >> 2) & 0xf);

            if (componentMask != 0)
            {
                var clearColor = _state.State.ClearColors;

                ColorF color = new ColorF(clearColor.Red, clearColor.Green, clearColor.Blue, clearColor.Alpha);

                _context.Renderer.Pipeline.ClearRenderTargetColor(index, layer, layerCount, componentMask, color);
            }

            if (clearDepth || clearStencil)
            {
                float depthValue = _state.State.ClearDepthValue;
                int stencilValue = (int)_state.State.ClearStencilValue;

                int stencilMask = 0;

                if (clearStencil)
                {
                    stencilMask = clearAffectedByStencilMask ? _state.State.StencilTestState.FrontMask : 0xff;
                }

                if (clipMismatch)
                {
                    _channel.TextureManager.UpdateRenderTargetDepthStencil();
                }

                _context.Renderer.Pipeline.ClearRenderTargetDepthStencil(
                    layer,
                    layerCount,
                    depthValue,
                    clearDepth,
                    stencilValue,
                    stencilMask);
            }

            if (needsCustomScissor)
            {
                engine.UpdateScissorState();
            }

            engine.UpdateRenderTargetState(useControl: true);

            if (renderEnable == ConditionalRenderEnabled.Host)
            {
                _context.Renderer.Pipeline.EndHostConditionalRendering();
            }
        }
    }
}

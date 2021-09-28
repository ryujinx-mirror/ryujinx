using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.Types;
using System.Text;

namespace Ryujinx.Graphics.Gpu.Engine.Threed
{
    /// <summary>
    /// Draw manager.
    /// </summary>
    class DrawManager
    {
        private readonly GpuContext _context;
        private readonly GpuChannel _channel;
        private readonly DeviceStateWithShadow<ThreedClassState> _state;
        private readonly DrawState _drawState;
        private bool _topologySet;

        private bool _instancedDrawPending;
        private bool _instancedIndexed;

        private int _instancedFirstIndex;
        private int _instancedFirstVertex;
        private int _instancedFirstInstance;
        private int _instancedIndexCount;
        private int _instancedDrawStateFirst;
        private int _instancedDrawStateCount;

        private int _instanceIndex;

        private const int IndexBufferCountMethodOffset = 0x5f8;

        /// <summary>
        /// Creates a new instance of the draw manager.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="channel">GPU channel</param>
        /// <param name="state">Channel state</param>
        /// <param name="drawState">Draw state</param>
        public DrawManager(GpuContext context, GpuChannel channel, DeviceStateWithShadow<ThreedClassState> state, DrawState drawState)
        {
            _context = context;
            _channel = channel;
            _state = state;
            _drawState = drawState;
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

            engine.UpdateState();

            bool instanced = _drawState.VsUsesInstanceId || _drawState.IsAnyVbInstanced;

            if (instanced)
            {
                _instancedDrawPending = true;

                _instancedIndexed = _drawState.DrawIndexed;

                _instancedFirstIndex = firstIndex;
                _instancedFirstVertex = (int)_state.State.FirstVertex;
                _instancedFirstInstance = (int)_state.State.FirstInstance;

                _instancedIndexCount = indexCount;

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

            if (_drawState.Topology != topology || !_topologySet)
            {
                _context.Renderer.Pipeline.SetPrimitiveTopology(topology);
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

            DrawEnd(engine, firstIndex, indexCount);

            _drawState.DrawIndexed = oldDrawIndexed;
        }

        /// <summary>
        /// Performs a indirect multi-draw, with parameters from a GPU buffer.
        /// </summary>
        /// <param name="engine">3D engine where this method is being called</param>
        /// <param name="topology">Primitive topology</param>
        /// <param name="indirectBuffer">GPU buffer with the draw parameters, such as count, first index, etc</param>
        /// <param name="parameterBuffer">GPU buffer with the draw count</param>
        /// <param name="maxDrawCount">Maximum number of draws that can be made</param>
        /// <param name="stride">Distance in bytes between each element on the <paramref name="indirectBuffer"/> array</param>
        public void MultiDrawIndirectCount(
            ThreedClass engine,
            int indexCount,
            PrimitiveTopology topology,
            BufferRange indirectBuffer,
            BufferRange parameterBuffer,
            int maxDrawCount,
            int stride)
        {
            engine.Write(IndexBufferCountMethodOffset * 4, indexCount);

            _context.Renderer.Pipeline.SetPrimitiveTopology(topology);
            _drawState.Topology = topology;
            _topologySet = true;

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

            _drawState.FirstIndex = _state.State.IndexBufferState.First;
            _drawState.IndexCount = indexCount;

            engine.UpdateState();

            if (_drawState.DrawIndexed)
            {
                _context.Renderer.Pipeline.MultiDrawIndexedIndirectCount(indirectBuffer, parameterBuffer, maxDrawCount, stride);
            }
            else
            {
                _context.Renderer.Pipeline.MultiDrawIndirectCount(indirectBuffer, parameterBuffer, maxDrawCount, stride);
            }

            _drawState.DrawIndexed = false;

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

                if (_instancedIndexed)
                {
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
        /// Which buffers should be cleared is also specified on the argument.
        /// </summary>
        /// <param name="engine">3D engine where this method is being called</param>
        /// <param name="argument">Method call argument</param>
        public void Clear(ThreedClass engine, int argument)
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

            // Scissor and rasterizer discard also affect clears.
            engine.UpdateState((1UL << StateUpdater.RasterizerStateIndex) | (1UL << StateUpdater.ScissorStateIndex));

            int index = (argument >> 6) & 0xf;

            engine.UpdateRenderTargetState(useControl: false, singleUse: index);

            _channel.TextureManager.UpdateRenderTargets();

            bool clearDepth = (argument & 1) != 0;
            bool clearStencil = (argument & 2) != 0;

            uint componentMask = (uint)((argument >> 2) & 0xf);

            if (componentMask != 0)
            {
                var clearColor = _state.State.ClearColors;

                ColorF color = new ColorF(clearColor.Red, clearColor.Green, clearColor.Blue, clearColor.Alpha);

                _context.Renderer.Pipeline.ClearRenderTargetColor(index, componentMask, color);
            }

            if (clearDepth || clearStencil)
            {
                float depthValue = _state.State.ClearDepthValue;
                int stencilValue = (int)_state.State.ClearStencilValue;

                int stencilMask = 0;

                if (clearStencil)
                {
                    stencilMask = _state.State.StencilTestState.FrontMask;
                }

                _context.Renderer.Pipeline.ClearRenderTargetDepthStencil(
                    depthValue,
                    clearDepth,
                    stencilValue,
                    stencilMask);
            }

            engine.UpdateRenderTargetState(useControl: true);

            if (renderEnable == ConditionalRenderEnabled.Host)
            {
                _context.Renderer.Pipeline.EndHostConditionalRendering();
            }
        }
    }
}

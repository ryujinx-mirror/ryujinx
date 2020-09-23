using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private bool _drawIndexed;

        private bool _instancedDrawPending;
        private bool _instancedIndexed;

        private int _instancedFirstIndex;
        private int _instancedFirstVertex;
        private int _instancedFirstInstance;
        private int _instancedIndexCount;
        private int _instancedDrawStateFirst;
        private int _instancedDrawStateCount;

        private int _instanceIndex;

        private IbStreamer _ibStreamer;

        /// <summary>
        /// Primitive topology of the current draw.
        /// </summary>
        public PrimitiveTopology Topology { get; private set; }

        /// <summary>
        /// Finishes the draw call.
        /// This draws geometry on the bound buffers based on the current GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void DrawEnd(GpuState state, int argument)
        {
            var indexBuffer = state.Get<IndexBufferState>(MethodOffset.IndexBufferState);

            DrawEnd(state, indexBuffer.First, indexBuffer.Count);
        }

        /// <summary>
        /// Finishes the draw call.
        /// This draws geometry on the bound buffers based on the current GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="firstIndex">Index of the first index buffer element used on the draw</param>
        /// <param name="indexCount">Number of index buffer elements used on the draw</param>
        private void DrawEnd(GpuState state, int firstIndex, int indexCount)
        {
            ConditionalRenderEnabled renderEnable = GetRenderEnable(state);

            if (renderEnable == ConditionalRenderEnabled.False || _instancedDrawPending)
            {
                if (renderEnable == ConditionalRenderEnabled.False)
                {
                    PerformDeferredDraws();
                }

                _drawIndexed = false;

                if (renderEnable == ConditionalRenderEnabled.Host)
                {
                    _context.Renderer.Pipeline.EndHostConditionalRendering();
                }

                return;
            }

            UpdateState(state, firstIndex, indexCount);

            bool instanced = _vsUsesInstanceId || _isAnyVbInstanced;

            if (instanced)
            {
                _instancedDrawPending = true;

                _instancedIndexed = _drawIndexed;

                _instancedFirstIndex = firstIndex;
                _instancedFirstVertex = state.Get<int>(MethodOffset.FirstVertex);
                _instancedFirstInstance = state.Get<int>(MethodOffset.FirstInstance);

                _instancedIndexCount = indexCount;

                var drawState = state.Get<VertexBufferDrawState>(MethodOffset.VertexBufferDrawState);

                _instancedDrawStateFirst = drawState.First;
                _instancedDrawStateCount = drawState.Count;

                _drawIndexed = false;

                if (renderEnable == ConditionalRenderEnabled.Host)
                {
                    _context.Renderer.Pipeline.EndHostConditionalRendering();
                }

                return;
            }

            int firstInstance = state.Get<int>(MethodOffset.FirstInstance);

            int inlineIndexCount = _ibStreamer.GetAndResetInlineIndexCount();

            if (inlineIndexCount != 0)
            {
                int firstVertex = state.Get<int>(MethodOffset.FirstVertex);

                BufferRange br = new BufferRange(_ibStreamer.GetInlineIndexBuffer(), 0, inlineIndexCount * 4);

                _context.Methods.BufferManager.SetIndexBuffer(br, IndexType.UInt);

                _context.Renderer.Pipeline.DrawIndexed(
                    inlineIndexCount,
                    1,
                    firstIndex,
                    firstVertex,
                    firstInstance);
            }
            else if (_drawIndexed)
            {
                int firstVertex = state.Get<int>(MethodOffset.FirstVertex);

                _context.Renderer.Pipeline.DrawIndexed(
                    indexCount,
                    1,
                    firstIndex,
                    firstVertex,
                    firstInstance);
            }
            else
            {
                var drawState = state.Get<VertexBufferDrawState>(MethodOffset.VertexBufferDrawState);

                _context.Renderer.Pipeline.Draw(
                    drawState.Count,
                    1,
                    drawState.First,
                    firstInstance);
            }

            _drawIndexed = false;

            if (renderEnable == ConditionalRenderEnabled.Host)
            {
                _context.Renderer.Pipeline.EndHostConditionalRendering();
            }
        }

        /// <summary>
        /// Starts draw.
        /// This sets primitive type and instanced draw parameters.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void DrawBegin(GpuState state, int argument)
        {
            bool incrementInstance = (argument & (1 << 26)) != 0;
            bool resetInstance     = (argument & (1 << 27)) == 0;

            PrimitiveType type = (PrimitiveType)(argument & 0xffff);

            PrimitiveTypeOverride typeOverride = state.Get<PrimitiveTypeOverride>(MethodOffset.PrimitiveTypeOverride);

            if (typeOverride != PrimitiveTypeOverride.Invalid)
            {
                DrawBegin(incrementInstance, resetInstance, typeOverride.Convert());
            }
            else
            {
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

            _context.Renderer.Pipeline.SetPrimitiveTopology(topology);

            Topology = topology;
        }

        /// <summary>
        /// Sets the index buffer count.
        /// This also sets internal state that indicates that the next draw is an indexed draw.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void SetIndexBufferCount(GpuState state, int argument)
        {
            _drawIndexed = true;
        }

        /// <summary>
        /// Performs a indexed draw with a low number of index buffer elements.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void DrawIndexedSmall(GpuState state, int argument)
        {
            DrawIndexedSmall(state, argument, false);
        }

        /// <summary>
        /// Performs a indexed draw with a low number of index buffer elements.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void DrawIndexedSmall2(GpuState state, int argument)
        {
            DrawIndexedSmall(state, argument);
        }

        /// <summary>
        /// Performs a indexed draw with a low number of index buffer elements,
        /// while also pre-incrementing the current instance value.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void DrawIndexedSmallIncInstance(GpuState state, int argument)
        {
            DrawIndexedSmall(state, argument, true);
        }

        /// <summary>
        /// Performs a indexed draw with a low number of index buffer elements,
        /// while also pre-incrementing the current instance value.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void DrawIndexedSmallIncInstance2(GpuState state, int argument)
        {
            DrawIndexedSmallIncInstance(state, argument);
        }

        /// <summary>
        /// Performs a indexed draw with a low number of index buffer elements,
        /// while optionally also pre-incrementing the current instance value.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        /// <param name="instanced">True to increment the current instance value, false otherwise</param>
        private void DrawIndexedSmall(GpuState state, int argument, bool instanced)
        {
            PrimitiveTypeOverride typeOverride = state.Get<PrimitiveTypeOverride>(MethodOffset.PrimitiveTypeOverride);

            DrawBegin(instanced, !instanced, typeOverride.Convert());

            int firstIndex = argument & 0xffff;
            int indexCount = (argument >> 16) & 0xfff;

            bool oldDrawIndexed = _drawIndexed;

            _drawIndexed = true;

            DrawEnd(state, firstIndex, indexCount);

            _drawIndexed = oldDrawIndexed;
        }

        /// <summary>
        /// Pushes four 8-bit index buffer elements.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void VbElementU8(GpuState state, int argument)
        {
            _ibStreamer.VbElementU8(_context.Renderer, argument);
        }

        /// <summary>
        /// Pushes two 16-bit index buffer elements.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void VbElementU16(GpuState state, int argument)
        {
            _ibStreamer.VbElementU16(_context.Renderer, argument);
        }

        /// <summary>
        /// Pushes one 32-bit index buffer element.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void VbElementU32(GpuState state, int argument)
        {
            _ibStreamer.VbElementU32(_context.Renderer, argument);
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
    }
}
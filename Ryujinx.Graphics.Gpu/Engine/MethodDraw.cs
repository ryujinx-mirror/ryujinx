using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.State;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private bool _drawIndexed;

        private int _firstIndex;
        private int _indexCount;

        private bool _instancedDrawPending;
        private bool _instancedIndexed;

        private int _instancedFirstIndex;
        private int _instancedFirstVertex;
        private int _instancedFirstInstance;
        private int _instancedIndexCount;
        private int _instancedDrawStateFirst;
        private int _instancedDrawStateCount;

        private int _instanceIndex;

        private BufferHandle _inlineIndexBuffer = BufferHandle.Null;
        private int _inlineIndexBufferSize;
        private int _inlineIndexCount;

        /// <summary>
        /// Primitive type of the current draw.
        /// </summary>
        public PrimitiveType PrimitiveType { get; private set; }

        /// <summary>
        /// Finishes draw call.
        /// This draws geometry on the bound buffers based on the current GPU state.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void DrawEnd(GpuState state, int argument)
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

            UpdateState(state);

            bool instanced = _vsUsesInstanceId || _isAnyVbInstanced;

            if (instanced)
            {
                _instancedDrawPending = true;

                _instancedIndexed = _drawIndexed;

                _instancedFirstIndex    = _firstIndex;
                _instancedFirstVertex   = state.Get<int>(MethodOffset.FirstVertex);
                _instancedFirstInstance = state.Get<int>(MethodOffset.FirstInstance);

                _instancedIndexCount = _indexCount;

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

            if (_inlineIndexCount != 0)
            {
                int firstVertex = state.Get<int>(MethodOffset.FirstVertex);

                BufferRange br = new BufferRange(_inlineIndexBuffer, 0, _inlineIndexCount * 4);

                _context.Methods.BufferManager.SetIndexBuffer(br, IndexType.UInt);

                _context.Renderer.Pipeline.DrawIndexed(
                    _inlineIndexCount,
                    1,
                    _firstIndex,
                    firstVertex,
                    firstInstance);

                _inlineIndexCount = 0;
            }
            else if (_drawIndexed)
            {
                int firstVertex = state.Get<int>(MethodOffset.FirstVertex);

                _context.Renderer.Pipeline.DrawIndexed(
                    _indexCount,
                    1,
                    _firstIndex,
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
            if ((argument & (1 << 26)) != 0)
            {
                _instanceIndex++;
            }
            else if ((argument & (1 << 27)) == 0)
            {
                PerformDeferredDraws();

                _instanceIndex = 0;
            }

            PrimitiveType type = (PrimitiveType)(argument & 0xffff);

            _context.Renderer.Pipeline.SetPrimitiveTopology(type.Convert());

            PrimitiveType = type;
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
        /// Pushes four 8-bit index buffer elements.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void VbElementU8(GpuState state, int argument)
        {
            byte i0 = (byte)argument;
            byte i1 = (byte)(argument >> 8);
            byte i2 = (byte)(argument >> 16);
            byte i3 = (byte)(argument >> 24);

            Span<uint> data = stackalloc uint[4];

            data[0] = i0;
            data[1] = i1;
            data[2] = i2;
            data[3] = i3;

            int offset = _inlineIndexCount * 4;

            _context.Renderer.SetBufferData(GetInlineIndexBuffer(offset), offset, MemoryMarshal.Cast<uint, byte>(data));

            _inlineIndexCount += 4;
        }

        /// <summary>
        /// Pushes two 16-bit index buffer elements.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void VbElementU16(GpuState state, int argument)
        {
            ushort i0 = (ushort)argument;
            ushort i1 = (ushort)(argument >> 16);

            Span<uint> data = stackalloc uint[2];

            data[0] = i0;
            data[1] = i1;

            int offset = _inlineIndexCount * 4;

            _context.Renderer.SetBufferData(GetInlineIndexBuffer(offset), offset, MemoryMarshal.Cast<uint, byte>(data));

            _inlineIndexCount += 2;
        }

        /// <summary>
        /// Pushes one 32-bit index buffer element.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void VbElementU32(GpuState state, int argument)
        {
            uint i0 = (uint)argument;

            Span<uint> data = stackalloc uint[1];

            data[0] = i0;

            int offset = _inlineIndexCount++ * 4;

            _context.Renderer.SetBufferData(GetInlineIndexBuffer(offset), offset, MemoryMarshal.Cast<uint, byte>(data));
        }

        /// <summary>
        /// Gets the handle of a buffer large enough to hold the data that will be written to <paramref name="offset"/>.
        /// </summary>
        /// <param name="offset">Offset where the data will be written</param>
        /// <returns>Buffer handle</returns>
        private BufferHandle GetInlineIndexBuffer(int offset)
        {
            // Calculate a reasonable size for the buffer that can fit all the data,
            // and that also won't require frequent resizes if we need to push more data.
            int size = BitUtils.AlignUp(offset + 0x10, 0x200);

            if (_inlineIndexBuffer == BufferHandle.Null)
            {
                _inlineIndexBuffer = _context.Renderer.CreateBuffer(size);
                _inlineIndexBufferSize = size;
            }
            else if (_inlineIndexBufferSize < size)
            {
                BufferHandle oldBuffer = _inlineIndexBuffer;
                int oldSize = _inlineIndexBufferSize;

                _inlineIndexBuffer = _context.Renderer.CreateBuffer(size);
                _inlineIndexBufferSize = size;

                _context.Renderer.Pipeline.CopyBuffer(oldBuffer, _inlineIndexBuffer, 0, 0, oldSize);
                _context.Renderer.DeleteBuffer(oldBuffer);
            }

            return _inlineIndexBuffer;
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
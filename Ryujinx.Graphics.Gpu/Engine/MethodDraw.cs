using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Gpu.Image;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private bool _drawIndexed;

        private int _firstIndex;
        private int _indexCount;

        private bool _instancedHasState;
        private bool _instancedIndexed;

        private int _instancedFirstIndex;
        private int _instancedFirstVertex;
        private int _instancedFirstInstance;
        private int _instancedIndexCount;
        private int _instancedDrawStateFirst;
        private int _instancedDrawStateCount;

        private int _instanceIndex;

        public PrimitiveType PrimitiveType { get; private set; }

        private void DrawEnd(int argument)
        {
            UpdateState();

            bool instanced = _vsUsesInstanceId || _isAnyVbInstanced;

            if (instanced)
            {
                if (!_instancedHasState)
                {
                    _instancedHasState = true;

                    _instancedIndexed = _drawIndexed;

                    _instancedFirstIndex    = _firstIndex;
                    _instancedFirstVertex   = _context.State.Get<int>(MethodOffset.FirstVertex);
                    _instancedFirstInstance = _context.State.Get<int>(MethodOffset.FirstInstance);

                    _instancedIndexCount = _indexCount;

                    var drawState = _context.State.Get<VertexBufferDrawState>(MethodOffset.VertexBufferDrawState);

                    _instancedDrawStateFirst = drawState.First;
                    _instancedDrawStateCount = drawState.Count;
                }

                return;
            }

            int firstInstance = _context.State.Get<int>(MethodOffset.FirstInstance);

            if (_drawIndexed)
            {
                _drawIndexed = false;

                int firstVertex = _context.State.Get<int>(MethodOffset.FirstVertex);

                _context.Renderer.Pipeline.DrawIndexed(
                    _indexCount,
                    1,
                    _firstIndex,
                    firstVertex,
                    firstInstance);
            }
            else
            {
                var drawState = _context.State.Get<VertexBufferDrawState>(MethodOffset.VertexBufferDrawState);

                _context.Renderer.Pipeline.Draw(
                    drawState.Count,
                    1,
                    drawState.First,
                    firstInstance);
            }
        }

        private void DrawBegin(int argument)
        {
            PrimitiveType type = (PrimitiveType)(argument & 0xffff);

            _context.Renderer.Pipeline.SetPrimitiveTopology(type.Convert());

            PrimitiveType = type;

            if ((argument & (1 << 26)) != 0)
            {
                _instanceIndex++;
            }
            else if ((argument & (1 << 27)) == 0)
            {
                _instanceIndex = 0;
            }
        }

        private void SetIndexBufferCount(int argument)
        {
            _drawIndexed = true;
        }

        public void PerformDeferredDraws()
        {
            // Perform any pending instanced draw.
            if (_instancedHasState)
            {
                _instancedHasState = false;

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
using Ryujinx.Graphics.GAL.Color;
using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private void Clear(int argument)
        {
            UpdateRenderTargetStateIfNeeded();

            _textureManager.CommitGraphicsBindings();

            bool clearDepth   = (argument & 1) != 0;
            bool clearStencil = (argument & 2) != 0;

            uint componentMask = (uint)((argument >> 2) & 0xf);

            int index = (argument >> 6) & 0xf;

            if (componentMask != 0)
            {
                var clearColor = _context.State.Get<ClearColors>(MethodOffset.ClearColors);

                ColorF color = new ColorF(
                    clearColor.Red,
                    clearColor.Green,
                    clearColor.Blue,
                    clearColor.Alpha);

                _context.Renderer.Pipeline.ClearRenderTargetColor(index, componentMask, color);
            }

            if (clearDepth || clearStencil)
            {
                float depthValue   = _context.State.Get<float>(MethodOffset.ClearDepthValue);
                int   stencilValue = _context.State.Get<int>  (MethodOffset.ClearStencilValue);

                int stencilMask = 0;

                if (clearStencil)
                {
                    stencilMask = _context.State.Get<StencilTestState>(MethodOffset.StencilTestState).FrontMask;
                }

                _context.Renderer.Pipeline.ClearRenderTargetDepthStencil(
                    depthValue,
                    clearDepth,
                    stencilValue,
                    stencilMask);
            }
        }
    }
}
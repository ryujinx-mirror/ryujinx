using Ryujinx.Graphics.GAL.Color;
using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private void Clear(int argument)
        {
            UpdateState();

            bool clearDepth   = (argument & 1) != 0;
            bool clearStencil = (argument & 2) != 0;

            uint componentMask = (uint)((argument >> 2) & 0xf);

            int index = (argument >> 6) & 0xf;

            if (componentMask != 0)
            {
                ClearColors clearColor = _context.State.GetClearColors();

                ColorF color = new ColorF(
                    clearColor.Red,
                    clearColor.Green,
                    clearColor.Blue,
                    clearColor.Alpha);

                _context.Renderer.GraphicsPipeline.ClearRenderTargetColor(
                    index,
                    componentMask,
                    color);
            }

            if (clearDepth || clearStencil)
            {
                float depthValue   = _context.State.GetClearDepthValue();
                int   stencilValue = _context.State.GetClearStencilValue();

                int stencilMask = 0;

                if (clearStencil)
                {
                    stencilMask = _context.State.GetStencilTestState().FrontMask;
                }

                _context.Renderer.GraphicsPipeline.ClearRenderTargetDepthStencil(
                    depthValue,
                    clearDepth,
                    stencilValue,
                    stencilMask);
            }
        }
    }
}
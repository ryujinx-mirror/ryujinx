using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.State;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        /// <summary>
        /// Clears the current color and depth-stencil buffers.
        /// Which buffers should be cleared is also specified on the argument.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void Clear(GpuState state, int argument)
        {
            ConditionalRenderEnabled renderEnable = GetRenderEnable(state);

            if (renderEnable == ConditionalRenderEnabled.False)
            {
                return;
            }

            // Scissor and rasterizer discard also affect clears.
            if (state.QueryModified(MethodOffset.ScissorState))
            {
                UpdateScissorState(state);
            }

            if (state.QueryModified(MethodOffset.RasterizeEnable))
            {
                UpdateRasterizerState(state);
            }

            int index = (argument >> 6) & 0xf;

            UpdateRenderTargetState(state, useControl: false, singleUse: index);

            TextureManager.UpdateRenderTargets();

            bool clearDepth   = (argument & 1) != 0;
            bool clearStencil = (argument & 2) != 0;

            uint componentMask = (uint)((argument >> 2) & 0xf);

            if (componentMask != 0)
            {
                var clearColor = state.Get<ClearColors>(MethodOffset.ClearColors);

                ColorF color = new ColorF(
                    clearColor.Red,
                    clearColor.Green,
                    clearColor.Blue,
                    clearColor.Alpha);

                _context.Renderer.Pipeline.ClearRenderTargetColor(index, componentMask, color);
            }

            if (clearDepth || clearStencil)
            {
                float depthValue   = state.Get<float>(MethodOffset.ClearDepthValue);
                int   stencilValue = state.Get<int>  (MethodOffset.ClearStencilValue);

                int stencilMask = 0;

                if (clearStencil)
                {
                    stencilMask = state.Get<StencilTestState>(MethodOffset.StencilTestState).FrontMask;
                }

                _context.Renderer.Pipeline.ClearRenderTargetDepthStencil(
                    depthValue,
                    clearDepth,
                    stencilValue,
                    stencilMask);
            }

            UpdateRenderTargetState(state, useControl: true);

            if (renderEnable == ConditionalRenderEnabled.Host)
            {
                _context.Renderer.Pipeline.EndHostConditionalRendering();
            }
        }
    }
}
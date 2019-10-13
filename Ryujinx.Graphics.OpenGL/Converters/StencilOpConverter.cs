using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class StencilOpConverter
    {
        public static StencilOp Convert(this GAL.DepthStencil.StencilOp op)
        {
            switch (op)
            {
                case GAL.DepthStencil.StencilOp.Keep:              return StencilOp.Keep;
                case GAL.DepthStencil.StencilOp.Zero:              return StencilOp.Zero;
                case GAL.DepthStencil.StencilOp.Replace:           return StencilOp.Replace;
                case GAL.DepthStencil.StencilOp.IncrementAndClamp: return StencilOp.Incr;
                case GAL.DepthStencil.StencilOp.DecrementAndClamp: return StencilOp.Decr;
                case GAL.DepthStencil.StencilOp.Invert:            return StencilOp.Invert;
                case GAL.DepthStencil.StencilOp.IncrementAndWrap:  return StencilOp.IncrWrap;
                case GAL.DepthStencil.StencilOp.DecrementAndWrap:  return StencilOp.DecrWrap;
            }

            return StencilOp.Keep;

            throw new ArgumentException($"Invalid stencil operation \"{op}\".");
        }
    }
}

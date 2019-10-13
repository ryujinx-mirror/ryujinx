using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL.Texture;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class DepthStencilModeConverter
    {
        public static All Convert(this DepthStencilMode mode)
        {
            switch (mode)
            {
                case DepthStencilMode.Depth:   return All.Depth;
                case DepthStencilMode.Stencil: return All.Stencil;
            }

            throw new ArgumentException($"Invalid depth stencil mode \"{mode}\".");
        }
    }
}

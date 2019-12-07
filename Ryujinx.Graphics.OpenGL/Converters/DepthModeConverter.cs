using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class DepthModeConverter
    {
        public static ClipDepthMode Convert(this DepthMode mode)
        {
            switch (mode)
            {
                case DepthMode.MinusOneToOne: return ClipDepthMode.NegativeOneToOne;
                case DepthMode.ZeroToOne:     return ClipDepthMode.ZeroToOne;
            }

            throw new ArgumentException($"Invalid depth mode \"{mode}\".");
        }
    }
}

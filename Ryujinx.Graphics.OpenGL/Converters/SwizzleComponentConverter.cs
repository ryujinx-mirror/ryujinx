using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL.Texture;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class SwizzleComponentConverter
    {
        public static All Convert(this SwizzleComponent swizzleComponent)
        {
            switch (swizzleComponent)
            {
                case SwizzleComponent.Zero:  return All.Zero;
                case SwizzleComponent.One:   return All.One;
                case SwizzleComponent.Red:   return All.Red;
                case SwizzleComponent.Green: return All.Green;
                case SwizzleComponent.Blue:  return All.Blue;
                case SwizzleComponent.Alpha: return All.Alpha;
            }

            throw new ArgumentException($"Invalid swizzle component \"{swizzleComponent}\".");
        }
    }
}

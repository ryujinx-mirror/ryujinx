using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL.Blend;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class BlendFactorConverter
    {
        public static All Convert(this BlendFactor factor)
        {
            switch (factor)
            {
                case BlendFactor.Zero:                  return All.Zero;
                case BlendFactor.One:                   return All.One;
                case BlendFactor.SrcColor:              return All.SrcColor;
                case BlendFactor.OneMinusSrcColor:      return All.OneMinusSrcColor;
                case BlendFactor.SrcAlpha:              return All.SrcAlpha;
                case BlendFactor.OneMinusSrcAlpha:      return All.OneMinusSrcAlpha;
                case BlendFactor.DstAlpha:              return All.DstAlpha;
                case BlendFactor.OneMinusDstAlpha:      return All.OneMinusDstAlpha;
                case BlendFactor.DstColor:              return All.DstColor;
                case BlendFactor.OneMinusDstColor:      return All.OneMinusDstColor;
                case BlendFactor.SrcAlphaSaturate:      return All.SrcAlphaSaturate;
                case BlendFactor.Src1Color:             return All.Src1Color;
                case BlendFactor.OneMinusSrc1Color:     return All.OneMinusSrc1Color;
                case BlendFactor.Src1Alpha:             return All.Src1Alpha;
                case BlendFactor.OneMinusSrc1Alpha:     return All.OneMinusSrc1Alpha;
                case BlendFactor.ConstantColor:         return All.ConstantColor;
                case BlendFactor.OneMinusConstantColor: return All.OneMinusConstantColor;
                case BlendFactor.ConstantAlpha:         return All.ConstantAlpha;
                case BlendFactor.OneMinusConstantAlpha: return All.OneMinusConstantAlpha;
            }

            return All.Zero;

            throw new ArgumentException($"Invalid blend factor \"{factor}\".");
        }
    }
}

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
                case BlendFactor.Zero:
                case BlendFactor.ZeroGl:
                    return All.Zero;
                case BlendFactor.One:
                case BlendFactor.OneGl:
                    return All.One;
                case BlendFactor.SrcColor:
                case BlendFactor.SrcColorGl:
                    return All.SrcColor;
                case BlendFactor.OneMinusSrcColor:
                case BlendFactor.OneMinusSrcColorGl:
                    return All.OneMinusSrcColor;
                case BlendFactor.SrcAlpha:
                case BlendFactor.SrcAlphaGl:
                    return All.SrcAlpha;
                case BlendFactor.OneMinusSrcAlpha:
                case BlendFactor.OneMinusSrcAlphaGl:
                    return All.OneMinusSrcAlpha;
                case BlendFactor.DstAlpha:
                case BlendFactor.DstAlphaGl:
                    return All.DstAlpha;
                case BlendFactor.OneMinusDstAlpha:
                case BlendFactor.OneMinusDstAlphaGl:
                    return All.OneMinusDstAlpha;
                case BlendFactor.DstColor:
                case BlendFactor.DstColorGl:
                    return All.DstColor;
                case BlendFactor.OneMinusDstColor:
                case BlendFactor.OneMinusDstColorGl:
                    return All.OneMinusDstColor;
                case BlendFactor.SrcAlphaSaturate:
                case BlendFactor.SrcAlphaSaturateGl:
                    return All.SrcAlphaSaturate;
                case BlendFactor.Src1Color:
                case BlendFactor.Src1ColorGl:
                    return All.Src1Color;
                case BlendFactor.OneMinusSrc1Color:
                case BlendFactor.OneMinusSrc1ColorGl:
                    return All.OneMinusSrc1Color;
                case BlendFactor.Src1Alpha:
                case BlendFactor.Src1AlphaGl:
                    return All.Src1Alpha;
                case BlendFactor.OneMinusSrc1Alpha:
                case BlendFactor.OneMinusSrc1AlphaGl:
                    return All.OneMinusSrc1Alpha;
                case BlendFactor.ConstantColor:
                    return All.ConstantColor;
                case BlendFactor.OneMinusConstantColor:
                    return All.OneMinusConstantColor;
                case BlendFactor.ConstantAlpha:
                    return All.ConstantAlpha;
                case BlendFactor.OneMinusConstantAlpha:
                    return All.OneMinusConstantAlpha;
            }

            return All.Zero;

            throw new ArgumentException($"Invalid blend factor \"{factor}\".");
        }
    }
}

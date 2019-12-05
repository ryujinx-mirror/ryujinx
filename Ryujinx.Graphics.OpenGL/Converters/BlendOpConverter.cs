using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL.Blend;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class BlendOpConverter
    {
        public static BlendEquationMode Convert(this BlendOp op)
        {
            switch (op)
            {
                case BlendOp.Add:
                case BlendOp.AddGl:
                    return BlendEquationMode.FuncAdd;
                case BlendOp.Subtract:
                case BlendOp.SubtractGl:
                    return BlendEquationMode.FuncSubtract;
                case BlendOp.ReverseSubtract:
                case BlendOp.ReverseSubtractGl:
                    return BlendEquationMode.FuncReverseSubtract;
                case BlendOp.Minimum:
                case BlendOp.MinimumGl:
                    return BlendEquationMode.Min;
                case BlendOp.Maximum:
                case BlendOp.MaximumGl:
                    return BlendEquationMode.Max;
            }

            return BlendEquationMode.FuncAdd;

            throw new ArgumentException($"Invalid blend operation \"{op}\".");
        }
    }
}

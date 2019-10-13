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
                case BlendOp.Add:             return BlendEquationMode.FuncAdd;
                case BlendOp.Subtract:        return BlendEquationMode.FuncSubtract;
                case BlendOp.ReverseSubtract: return BlendEquationMode.FuncReverseSubtract;
                case BlendOp.Minimum:         return BlendEquationMode.Min;
                case BlendOp.Maximum:         return BlendEquationMode.Max;
            }

            return BlendEquationMode.FuncAdd;

            throw new ArgumentException($"Invalid blend operation \"{op}\".");
        }
    }
}

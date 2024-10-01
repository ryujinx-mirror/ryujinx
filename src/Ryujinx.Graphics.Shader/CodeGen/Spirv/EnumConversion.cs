using System;
using static Spv.Specification;

namespace Ryujinx.Graphics.Shader.CodeGen.Spirv
{
    static class EnumConversion
    {
        public static ExecutionModel Convert(this ShaderStage stage)
        {
            return stage switch
            {
                ShaderStage.Compute => ExecutionModel.GLCompute,
                ShaderStage.Vertex => ExecutionModel.Vertex,
                ShaderStage.TessellationControl => ExecutionModel.TessellationControl,
                ShaderStage.TessellationEvaluation => ExecutionModel.TessellationEvaluation,
                ShaderStage.Geometry => ExecutionModel.Geometry,
                ShaderStage.Fragment => ExecutionModel.Fragment,
                _ => throw new ArgumentException($"Invalid shader stage \"{stage}\"."),
            };
        }
    }
}

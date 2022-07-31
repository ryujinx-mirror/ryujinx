using Ryujinx.Graphics.Shader.StructuredIr;
using Ryujinx.Graphics.Shader.Translation;
using System;
using static Spv.Specification;

namespace Ryujinx.Graphics.Shader.CodeGen.Spirv
{
    static class EnumConversion
    {
        public static AggregateType Convert(this VariableType type)
        {
            return type switch
            {
                VariableType.None => AggregateType.Void,
                VariableType.Bool => AggregateType.Bool,
                VariableType.F32 => AggregateType.FP32,
                VariableType.F64 => AggregateType.FP64,
                VariableType.S32 => AggregateType.S32,
                VariableType.U32 => AggregateType.U32,
                _ => throw new ArgumentException($"Invalid variable type \"{type}\".")
            };
        }

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
                _ => throw new ArgumentException($"Invalid shader stage \"{stage}\".")
            };
        }
    }
}

using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;
using System;
using static Spv.Specification;

namespace Ryujinx.Graphics.Shader.CodeGen.Spirv
{
    static class IoMap
    {
        // At least 16 attributes are guaranteed by the spec.
        private const int MaxAttributes = 16;

        public static (BuiltIn, AggregateType) GetSpirvBuiltIn(IoVariable ioVariable)
        {
            return ioVariable switch
            {
                IoVariable.BaseInstance => (BuiltIn.BaseInstance, AggregateType.S32),
                IoVariable.BaseVertex => (BuiltIn.BaseVertex, AggregateType.S32),
                IoVariable.ClipDistance => (BuiltIn.ClipDistance, AggregateType.Array | AggregateType.FP32),
                IoVariable.CtaId => (BuiltIn.WorkgroupId, AggregateType.Vector3 | AggregateType.U32),
                IoVariable.DrawIndex => (BuiltIn.DrawIndex, AggregateType.S32),
                IoVariable.FragmentCoord => (BuiltIn.FragCoord, AggregateType.Vector4 | AggregateType.FP32),
                IoVariable.FragmentOutputDepth => (BuiltIn.FragDepth, AggregateType.FP32),
                IoVariable.FrontFacing => (BuiltIn.FrontFacing, AggregateType.Bool),
                IoVariable.GlobalId => (BuiltIn.GlobalInvocationId, AggregateType.Vector3 | AggregateType.U32),
                IoVariable.InstanceId => (BuiltIn.InstanceId, AggregateType.S32),
                IoVariable.InstanceIndex => (BuiltIn.InstanceIndex, AggregateType.S32),
                IoVariable.InvocationId => (BuiltIn.InvocationId, AggregateType.S32),
                IoVariable.Layer => (BuiltIn.Layer, AggregateType.S32),
                IoVariable.PatchVertices => (BuiltIn.PatchVertices, AggregateType.S32),
                IoVariable.PointCoord => (BuiltIn.PointCoord, AggregateType.Vector2 | AggregateType.FP32),
                IoVariable.PointSize => (BuiltIn.PointSize, AggregateType.FP32),
                IoVariable.Position => (BuiltIn.Position, AggregateType.Vector4 | AggregateType.FP32),
                IoVariable.PrimitiveId => (BuiltIn.PrimitiveId, AggregateType.S32),
                IoVariable.SubgroupEqMask => (BuiltIn.SubgroupEqMask, AggregateType.Vector4 | AggregateType.U32),
                IoVariable.SubgroupGeMask => (BuiltIn.SubgroupGeMask, AggregateType.Vector4 | AggregateType.U32),
                IoVariable.SubgroupGtMask => (BuiltIn.SubgroupGtMask, AggregateType.Vector4 | AggregateType.U32),
                IoVariable.SubgroupLaneId => (BuiltIn.SubgroupLocalInvocationId, AggregateType.U32),
                IoVariable.SubgroupLeMask => (BuiltIn.SubgroupLeMask, AggregateType.Vector4 | AggregateType.U32),
                IoVariable.SubgroupLtMask => (BuiltIn.SubgroupLtMask, AggregateType.Vector4 | AggregateType.U32),
                IoVariable.TessellationCoord => (BuiltIn.TessCoord, AggregateType.Vector3 | AggregateType.FP32),
                IoVariable.TessellationLevelInner => (BuiltIn.TessLevelInner, AggregateType.Array | AggregateType.FP32),
                IoVariable.TessellationLevelOuter => (BuiltIn.TessLevelOuter, AggregateType.Array | AggregateType.FP32),
                IoVariable.ThreadId => (BuiltIn.LocalInvocationId, AggregateType.Vector3 | AggregateType.U32),
                IoVariable.ThreadKill => (BuiltIn.HelperInvocation, AggregateType.Bool),
                IoVariable.VertexId => (BuiltIn.VertexId, AggregateType.S32),
                IoVariable.VertexIndex => (BuiltIn.VertexIndex, AggregateType.S32),
                IoVariable.ViewportIndex => (BuiltIn.ViewportIndex, AggregateType.S32),
                IoVariable.ViewportMask => (BuiltIn.ViewportMaskNV, AggregateType.Array | AggregateType.S32),
                _ => (default, AggregateType.Invalid),
            };
        }

        public static int GetSpirvBuiltInArrayLength(IoVariable ioVariable)
        {
            return ioVariable switch
            {
                IoVariable.ClipDistance => 8,
                IoVariable.TessellationLevelInner => 2,
                IoVariable.TessellationLevelOuter => 4,
                IoVariable.ViewportMask => 1,
                IoVariable.UserDefined => MaxAttributes,
                _ => 1,
            };
        }

        public static bool IsPerVertex(IoVariable ioVariable, ShaderStage stage, bool isOutput)
        {
            switch (ioVariable)
            {
                case IoVariable.Layer:
                case IoVariable.ViewportIndex:
                case IoVariable.PointSize:
                case IoVariable.Position:
                case IoVariable.UserDefined:
                case IoVariable.ClipDistance:
                case IoVariable.PointCoord:
                case IoVariable.ViewportMask:
                    return !isOutput &&
                           stage is ShaderStage.TessellationControl or ShaderStage.TessellationEvaluation or ShaderStage.Geometry;
            }

            return false;
        }

        public static bool IsPerVertexBuiltIn(IoVariable ioVariable)
        {
            switch (ioVariable)
            {
                case IoVariable.Position:
                case IoVariable.PointSize:
                case IoVariable.ClipDistance:
                    return true;
            }

            return false;
        }

        public static bool IsPerVertexArrayBuiltIn(StorageKind storageKind, ShaderStage stage)
        {
            if (storageKind == StorageKind.Output)
            {
                return stage == ShaderStage.TessellationControl;
            }
            else
            {
                return stage == ShaderStage.TessellationControl ||
                       stage == ShaderStage.TessellationEvaluation ||
                       stage == ShaderStage.Geometry;
            }
        }

        public static int GetPerVertexStructFieldIndex(IoVariable ioVariable)
        {
            return ioVariable switch
            {
                IoVariable.Position => 0,
                IoVariable.PointSize => 1,
                IoVariable.ClipDistance => 2,
                _ => throw new ArgumentException($"Invalid built-in variable {ioVariable}.")
            };
        }
    }
}

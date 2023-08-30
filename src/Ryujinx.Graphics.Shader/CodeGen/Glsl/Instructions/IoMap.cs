using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;
using System.Globalization;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions
{
    static class IoMap
    {
        public static (string, AggregateType) GetGlslVariable(
            ShaderDefinitions definitions,
            HostCapabilities hostCapabilities,
            IoVariable ioVariable,
            int location,
            int component,
            bool isOutput,
            bool isPerPatch)
        {
            return ioVariable switch
            {
                IoVariable.BackColorDiffuse => ("gl_BackColor", AggregateType.Vector4 | AggregateType.FP32), // Deprecated.
                IoVariable.BackColorSpecular => ("gl_BackSecondaryColor", AggregateType.Vector4 | AggregateType.FP32), // Deprecated.
                IoVariable.BaseInstance => ("gl_BaseInstanceARB", AggregateType.S32),
                IoVariable.BaseVertex => ("gl_BaseVertexARB", AggregateType.S32),
                IoVariable.ClipDistance => ("gl_ClipDistance", AggregateType.Array | AggregateType.FP32),
                IoVariable.CtaId => ("gl_WorkGroupID", AggregateType.Vector3 | AggregateType.U32),
                IoVariable.DrawIndex => ("gl_DrawIDARB", AggregateType.S32),
                IoVariable.FogCoord => ("gl_FogFragCoord", AggregateType.FP32), // Deprecated.
                IoVariable.FragmentCoord => ("gl_FragCoord", AggregateType.Vector4 | AggregateType.FP32),
                IoVariable.FragmentOutputColor => GetFragmentOutputColorVariableName(definitions, location),
                IoVariable.FragmentOutputDepth => ("gl_FragDepth", AggregateType.FP32),
                IoVariable.FrontColorDiffuse => ("gl_FrontColor", AggregateType.Vector4 | AggregateType.FP32), // Deprecated.
                IoVariable.FrontColorSpecular => ("gl_FrontSecondaryColor", AggregateType.Vector4 | AggregateType.FP32), // Deprecated.
                IoVariable.FrontFacing => ("gl_FrontFacing", AggregateType.Bool),
                IoVariable.GlobalId => ("gl_GlobalInvocationID", AggregateType.Vector3 | AggregateType.U32),
                IoVariable.InstanceId => ("gl_InstanceID", AggregateType.S32),
                IoVariable.InstanceIndex => ("gl_InstanceIndex", AggregateType.S32),
                IoVariable.InvocationId => ("gl_InvocationID", AggregateType.S32),
                IoVariable.Layer => ("gl_Layer", AggregateType.S32),
                IoVariable.PatchVertices => ("gl_PatchVerticesIn", AggregateType.S32),
                IoVariable.PointCoord => ("gl_PointCoord", AggregateType.Vector2 | AggregateType.FP32),
                IoVariable.PointSize => ("gl_PointSize", AggregateType.FP32),
                IoVariable.Position => ("gl_Position", AggregateType.Vector4 | AggregateType.FP32),
                IoVariable.PrimitiveId => GetPrimitiveIdVariableName(definitions.Stage, isOutput),
                IoVariable.SubgroupEqMask => GetSubgroupMaskVariableName(hostCapabilities.SupportsShaderBallot, "Eq"),
                IoVariable.SubgroupGeMask => GetSubgroupMaskVariableName(hostCapabilities.SupportsShaderBallot, "Ge"),
                IoVariable.SubgroupGtMask => GetSubgroupMaskVariableName(hostCapabilities.SupportsShaderBallot, "Gt"),
                IoVariable.SubgroupLaneId => GetSubgroupInvocationIdVariableName(hostCapabilities.SupportsShaderBallot),
                IoVariable.SubgroupLeMask => GetSubgroupMaskVariableName(hostCapabilities.SupportsShaderBallot, "Le"),
                IoVariable.SubgroupLtMask => GetSubgroupMaskVariableName(hostCapabilities.SupportsShaderBallot, "Lt"),
                IoVariable.TessellationCoord => ("gl_TessCoord", AggregateType.Vector3 | AggregateType.FP32),
                IoVariable.TessellationLevelInner => ("gl_TessLevelInner", AggregateType.Array | AggregateType.FP32),
                IoVariable.TessellationLevelOuter => ("gl_TessLevelOuter", AggregateType.Array | AggregateType.FP32),
                IoVariable.TextureCoord => ("gl_TexCoord", AggregateType.Array | AggregateType.Vector4 | AggregateType.FP32), // Deprecated.
                IoVariable.ThreadId => ("gl_LocalInvocationID", AggregateType.Vector3 | AggregateType.U32),
                IoVariable.ThreadKill => ("gl_HelperInvocation", AggregateType.Bool),
                IoVariable.UserDefined => GetUserDefinedVariableName(definitions, location, component, isOutput, isPerPatch),
                IoVariable.VertexId => ("gl_VertexID", AggregateType.S32),
                IoVariable.VertexIndex => ("gl_VertexIndex", AggregateType.S32),
                IoVariable.ViewportIndex => ("gl_ViewportIndex", AggregateType.S32),
                IoVariable.ViewportMask => ("gl_ViewportMask", AggregateType.Array | AggregateType.S32),
                _ => (null, AggregateType.Invalid),
            };
        }

        public static bool IsPerVertexBuiltIn(ShaderStage stage, IoVariable ioVariable, bool isOutput)
        {
            switch (ioVariable)
            {
                case IoVariable.Layer:
                case IoVariable.ViewportIndex:
                case IoVariable.PointSize:
                case IoVariable.Position:
                case IoVariable.ClipDistance:
                case IoVariable.PointCoord:
                case IoVariable.ViewportMask:
                    if (isOutput)
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

            return false;
        }

        private static (string, AggregateType) GetFragmentOutputColorVariableName(ShaderDefinitions definitions, int location)
        {
            if (location < 0)
            {
                return (DefaultNames.OAttributePrefix, definitions.GetFragmentOutputColorType(0));
            }

            string name = DefaultNames.OAttributePrefix + location.ToString(CultureInfo.InvariantCulture);

            return (name, definitions.GetFragmentOutputColorType(location));
        }

        private static (string, AggregateType) GetPrimitiveIdVariableName(ShaderStage stage, bool isOutput)
        {
            // The geometry stage has an additional gl_PrimitiveIDIn variable.
            return (isOutput || stage != ShaderStage.Geometry ? "gl_PrimitiveID" : "gl_PrimitiveIDIn", AggregateType.S32);
        }

        private static (string, AggregateType) GetSubgroupMaskVariableName(bool supportsShaderBallot, string cc)
        {
            return supportsShaderBallot
                ? ($"unpackUint2x32(gl_SubGroup{cc}MaskARB)", AggregateType.Vector2 | AggregateType.U32)
                : ($"gl_Subgroup{cc}Mask", AggregateType.Vector4 | AggregateType.U32);
        }

        private static (string, AggregateType) GetSubgroupInvocationIdVariableName(bool supportsShaderBallot)
        {
            return supportsShaderBallot
                ? ("gl_SubGroupInvocationARB", AggregateType.U32)
                : ("gl_SubgroupInvocationID", AggregateType.U32);
        }

        private static (string, AggregateType) GetUserDefinedVariableName(ShaderDefinitions definitions, int location, int component, bool isOutput, bool isPerPatch)
        {
            string name = isPerPatch
                ? DefaultNames.PerPatchAttributePrefix
                : (isOutput ? DefaultNames.OAttributePrefix : DefaultNames.IAttributePrefix);

            if (location < 0)
            {
                return (name, definitions.GetUserDefinedType(0, isOutput));
            }

            name += location.ToString(CultureInfo.InvariantCulture);

            if (definitions.HasPerLocationInputOrOutputComponent(IoVariable.UserDefined, location, component, isOutput))
            {
                name += "_" + "xyzw"[component & 3];
            }

            return (name, definitions.GetUserDefinedType(location, isOutput));
        }
    }
}

using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

using static Ryujinx.Graphics.Shader.StructuredIr.InstructionInfo;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl
{
    class OperandManager
    {
        private static readonly string[] StagePrefixes = new string[] { "cp", "vp", "tcp", "tep", "gp", "fp" };

        private readonly struct BuiltInAttribute
        {
            public string Name { get; }

            public AggregateType Type { get; }

            public BuiltInAttribute(string name, AggregateType type)
            {
                Name = name;
                Type = type;
            }
        }

        private static Dictionary<int, BuiltInAttribute> _builtInAttributes = new Dictionary<int, BuiltInAttribute>()
        {
            { AttributeConsts.Layer,         new BuiltInAttribute("gl_Layer",           AggregateType.S32)  },
            { AttributeConsts.PointSize,     new BuiltInAttribute("gl_PointSize",       AggregateType.FP32)  },
            { AttributeConsts.PositionX,     new BuiltInAttribute("gl_Position.x",      AggregateType.FP32)  },
            { AttributeConsts.PositionY,     new BuiltInAttribute("gl_Position.y",      AggregateType.FP32)  },
            { AttributeConsts.PositionZ,     new BuiltInAttribute("gl_Position.z",      AggregateType.FP32)  },
            { AttributeConsts.PositionW,     new BuiltInAttribute("gl_Position.w",      AggregateType.FP32)  },
            { AttributeConsts.ClipDistance0, new BuiltInAttribute("gl_ClipDistance[0]", AggregateType.FP32)  },
            { AttributeConsts.ClipDistance1, new BuiltInAttribute("gl_ClipDistance[1]", AggregateType.FP32)  },
            { AttributeConsts.ClipDistance2, new BuiltInAttribute("gl_ClipDistance[2]", AggregateType.FP32)  },
            { AttributeConsts.ClipDistance3, new BuiltInAttribute("gl_ClipDistance[3]", AggregateType.FP32)  },
            { AttributeConsts.ClipDistance4, new BuiltInAttribute("gl_ClipDistance[4]", AggregateType.FP32)  },
            { AttributeConsts.ClipDistance5, new BuiltInAttribute("gl_ClipDistance[5]", AggregateType.FP32)  },
            { AttributeConsts.ClipDistance6, new BuiltInAttribute("gl_ClipDistance[6]", AggregateType.FP32)  },
            { AttributeConsts.ClipDistance7, new BuiltInAttribute("gl_ClipDistance[7]", AggregateType.FP32)  },
            { AttributeConsts.PointCoordX,   new BuiltInAttribute("gl_PointCoord.x",    AggregateType.FP32)  },
            { AttributeConsts.PointCoordY,   new BuiltInAttribute("gl_PointCoord.y",    AggregateType.FP32)  },
            { AttributeConsts.TessCoordX,    new BuiltInAttribute("gl_TessCoord.x",     AggregateType.FP32)  },
            { AttributeConsts.TessCoordY,    new BuiltInAttribute("gl_TessCoord.y",     AggregateType.FP32)  },
            { AttributeConsts.InstanceId,    new BuiltInAttribute("gl_InstanceID",      AggregateType.S32)  },
            { AttributeConsts.VertexId,      new BuiltInAttribute("gl_VertexID",        AggregateType.S32)  },
            { AttributeConsts.BaseInstance,  new BuiltInAttribute("gl_BaseInstanceARB", AggregateType.S32)  },
            { AttributeConsts.BaseVertex,    new BuiltInAttribute("gl_BaseVertexARB",   AggregateType.S32)  },
            { AttributeConsts.InstanceIndex, new BuiltInAttribute("gl_InstanceIndex",   AggregateType.S32)  },
            { AttributeConsts.VertexIndex,   new BuiltInAttribute("gl_VertexIndex",     AggregateType.S32)  },
            { AttributeConsts.DrawIndex,     new BuiltInAttribute("gl_DrawIDARB",       AggregateType.S32)  },
            { AttributeConsts.FrontFacing,   new BuiltInAttribute("gl_FrontFacing",     AggregateType.Bool) },

            // Special.
            { AttributeConsts.FragmentOutputDepth, new BuiltInAttribute("gl_FragDepth",           AggregateType.FP32)  },
            { AttributeConsts.ThreadKill,          new BuiltInAttribute("gl_HelperInvocation",    AggregateType.Bool) },
            { AttributeConsts.ThreadIdX,           new BuiltInAttribute("gl_LocalInvocationID.x", AggregateType.U32)  },
            { AttributeConsts.ThreadIdY,           new BuiltInAttribute("gl_LocalInvocationID.y", AggregateType.U32)  },
            { AttributeConsts.ThreadIdZ,           new BuiltInAttribute("gl_LocalInvocationID.z", AggregateType.U32)  },
            { AttributeConsts.CtaIdX,              new BuiltInAttribute("gl_WorkGroupID.x",       AggregateType.U32)  },
            { AttributeConsts.CtaIdY,              new BuiltInAttribute("gl_WorkGroupID.y",       AggregateType.U32)  },
            { AttributeConsts.CtaIdZ,              new BuiltInAttribute("gl_WorkGroupID.z",       AggregateType.U32)  },
            { AttributeConsts.LaneId,              new BuiltInAttribute(null,                     AggregateType.U32)  },
            { AttributeConsts.InvocationId,        new BuiltInAttribute("gl_InvocationID",        AggregateType.S32)  },
            { AttributeConsts.PrimitiveId,         new BuiltInAttribute("gl_PrimitiveID",         AggregateType.S32)  },
            { AttributeConsts.PatchVerticesIn,     new BuiltInAttribute("gl_PatchVerticesIn",     AggregateType.S32)  },
            { AttributeConsts.EqMask,              new BuiltInAttribute(null,                     AggregateType.U32)  },
            { AttributeConsts.GeMask,              new BuiltInAttribute(null,                     AggregateType.U32)  },
            { AttributeConsts.GtMask,              new BuiltInAttribute(null,                     AggregateType.U32)  },
            { AttributeConsts.LeMask,              new BuiltInAttribute(null,                     AggregateType.U32)  },
            { AttributeConsts.LtMask,              new BuiltInAttribute(null,                     AggregateType.U32)  },

            // Support uniforms.
            { AttributeConsts.FragmentOutputIsBgraBase + 0,  new BuiltInAttribute($"{DefaultNames.SupportBlockIsBgraName}[0]",  AggregateType.Bool) },
            { AttributeConsts.FragmentOutputIsBgraBase + 4,  new BuiltInAttribute($"{DefaultNames.SupportBlockIsBgraName}[1]",  AggregateType.Bool) },
            { AttributeConsts.FragmentOutputIsBgraBase + 8,  new BuiltInAttribute($"{DefaultNames.SupportBlockIsBgraName}[2]",  AggregateType.Bool) },
            { AttributeConsts.FragmentOutputIsBgraBase + 12, new BuiltInAttribute($"{DefaultNames.SupportBlockIsBgraName}[3]",  AggregateType.Bool) },
            { AttributeConsts.FragmentOutputIsBgraBase + 16, new BuiltInAttribute($"{DefaultNames.SupportBlockIsBgraName}[4]",  AggregateType.Bool) },
            { AttributeConsts.FragmentOutputIsBgraBase + 20, new BuiltInAttribute($"{DefaultNames.SupportBlockIsBgraName}[5]",  AggregateType.Bool) },
            { AttributeConsts.FragmentOutputIsBgraBase + 24, new BuiltInAttribute($"{DefaultNames.SupportBlockIsBgraName}[6]",  AggregateType.Bool) },
            { AttributeConsts.FragmentOutputIsBgraBase + 28, new BuiltInAttribute($"{DefaultNames.SupportBlockIsBgraName}[7]",  AggregateType.Bool) },

            { AttributeConsts.SupportBlockViewInverseX,  new BuiltInAttribute($"{DefaultNames.SupportBlockViewportInverse}.x",  AggregateType.FP32) },
            { AttributeConsts.SupportBlockViewInverseY,  new BuiltInAttribute($"{DefaultNames.SupportBlockViewportInverse}.y",  AggregateType.FP32) }
        };

        private Dictionary<AstOperand, string> _locals;

        public OperandManager()
        {
            _locals = new Dictionary<AstOperand, string>();
        }

        public string DeclareLocal(AstOperand operand)
        {
            string name = $"{DefaultNames.LocalNamePrefix}_{_locals.Count}";

            _locals.Add(operand, name);

            return name;
        }

        public string GetExpression(CodeGenContext context, AstOperand operand)
        {
            return operand.Type switch
            {
                OperandType.Argument => GetArgumentName(operand.Value),
                OperandType.Attribute => GetAttributeName(context, operand.Value, perPatch: false),
                OperandType.AttributePerPatch => GetAttributeName(context, operand.Value, perPatch: true),
                OperandType.Constant => NumberFormatter.FormatInt(operand.Value),
                OperandType.ConstantBuffer => GetConstantBufferName(operand, context.Config),
                OperandType.LocalVariable => _locals[operand],
                OperandType.Undefined => DefaultNames.UndefinedName,
                _ => throw new ArgumentException($"Invalid operand type \"{operand.Type}\".")
            };
        }

        private static string GetConstantBufferName(AstOperand operand, ShaderConfig config)
        {
            return GetConstantBufferName(operand.CbufSlot, operand.CbufOffset, config.Stage, config.UsedFeatures.HasFlag(FeatureFlags.CbIndexing));
        }

        public static string GetConstantBufferName(int slot, int offset, ShaderStage stage, bool cbIndexable)
        {
            return $"{GetUbName(stage, slot, cbIndexable)}[{offset >> 2}].{GetSwizzleMask(offset & 3)}";
        }

        private static string GetVec4Indexed(string vectorName, string indexExpr, bool indexElement)
        {
            if (indexElement)
            {
                return $"{vectorName}[{indexExpr}]";
            }

            string result = $"{vectorName}.x";
            for (int i = 1; i < 4; i++)
            {
                result = $"(({indexExpr}) == {i}) ? ({vectorName}.{GetSwizzleMask(i)}) : ({result})";
            }
            return $"({result})";
        }

        public static string GetConstantBufferName(int slot, string offsetExpr, ShaderStage stage, bool cbIndexable, bool indexElement)
        {
            return GetVec4Indexed(GetUbName(stage, slot, cbIndexable) + $"[{offsetExpr} >> 2]", offsetExpr + " & 3", indexElement);
        }

        public static string GetConstantBufferName(string slotExpr, string offsetExpr, ShaderStage stage, bool indexElement)
        {
            return GetVec4Indexed(GetUbName(stage, slotExpr) + $"[{offsetExpr} >> 2]", offsetExpr + " & 3", indexElement);
        }

        public static string GetOutAttributeName(CodeGenContext context, int value, bool perPatch)
        {
            return GetAttributeName(context, value, perPatch, isOutAttr: true);
        }

        public static string GetAttributeName(CodeGenContext context, int value, bool perPatch, bool isOutAttr = false, string indexExpr = "0")
        {
            ShaderConfig config = context.Config;

            if ((value & AttributeConsts.LoadOutputMask) != 0)
            {
                isOutAttr = true;
            }

            value &= AttributeConsts.Mask & ~3;
            char swzMask = GetSwizzleMask((value >> 2) & 3);

            if (perPatch)
            {
                if (value >= AttributeConsts.UserAttributePerPatchBase && value < AttributeConsts.UserAttributePerPatchEnd)
                {
                    value -= AttributeConsts.UserAttributePerPatchBase;

                    return $"{DefaultNames.PerPatchAttributePrefix}{(value >> 4)}.{swzMask}";
                }
                else if (value < AttributeConsts.UserAttributePerPatchBase)
                {
                    return value switch
                    {
                        AttributeConsts.TessLevelOuter0 => "gl_TessLevelOuter[0]",
                        AttributeConsts.TessLevelOuter1 => "gl_TessLevelOuter[1]",
                        AttributeConsts.TessLevelOuter2 => "gl_TessLevelOuter[2]",
                        AttributeConsts.TessLevelOuter3 => "gl_TessLevelOuter[3]",
                        AttributeConsts.TessLevelInner0 => "gl_TessLevelInner[0]",
                        AttributeConsts.TessLevelInner1 => "gl_TessLevelInner[1]",
                        _ => null
                    };
                }
            }
            else if (value >= AttributeConsts.UserAttributeBase && value < AttributeConsts.UserAttributeEnd)
            {
                int attrOffset = value;
                value -= AttributeConsts.UserAttributeBase;

                string prefix = isOutAttr
                    ? DefaultNames.OAttributePrefix
                    : DefaultNames.IAttributePrefix;

                bool indexable = config.UsedFeatures.HasFlag(isOutAttr ? FeatureFlags.OaIndexing : FeatureFlags.IaIndexing);

                if (indexable)
                {
                    string name = prefix;

                    if (config.Stage == ShaderStage.Geometry && !isOutAttr)
                    {
                        name += $"[{indexExpr}]";
                    }

                    return name + $"[{(value >> 4)}]." + swzMask;
                }
                else if (config.TransformFeedbackEnabled &&
                    ((config.LastInVertexPipeline && isOutAttr) ||
                    (config.Stage == ShaderStage.Fragment && !isOutAttr)))
                {
                    int components = config.LastInPipeline ? context.Info.GetTransformFeedbackOutputComponents(attrOffset) : 1;
                    string name = components > 1 ? $"{prefix}{(value >> 4)}" : $"{prefix}{(value >> 4)}_{swzMask}";

                    if (AttributeInfo.IsArrayAttributeGlsl(config.Stage, isOutAttr))
                    {
                        name += isOutAttr ? "[gl_InvocationID]" : $"[{indexExpr}]";
                    }

                    return components > 1 ? name + '.' + swzMask : name;
                }
                else
                {
                    string name = $"{prefix}{(value >> 4)}";

                    if (AttributeInfo.IsArrayAttributeGlsl(config.Stage, isOutAttr))
                    {
                        name += isOutAttr ? "[gl_InvocationID]" : $"[{indexExpr}]";
                    }

                    return name + '.' + swzMask;
                }
            }
            else
            {
                if (value >= AttributeConsts.FragmentOutputColorBase && value < AttributeConsts.FragmentOutputColorEnd)
                {
                    value -= AttributeConsts.FragmentOutputColorBase;

                    return $"{DefaultNames.OAttributePrefix}{(value >> 4)}.{swzMask}";
                }
                else if (_builtInAttributes.TryGetValue(value, out BuiltInAttribute builtInAttr))
                {
                    string subgroupMask = value switch
                    {
                        AttributeConsts.EqMask => "Eq",
                        AttributeConsts.GeMask => "Ge",
                        AttributeConsts.GtMask => "Gt",
                        AttributeConsts.LeMask => "Le",
                        AttributeConsts.LtMask => "Lt",
                        _ => null
                    };

                    if (subgroupMask != null)
                    {
                        return config.GpuAccessor.QueryHostSupportsShaderBallot()
                            ? $"unpackUint2x32(gl_SubGroup{subgroupMask}MaskARB).x"
                            : $"gl_Subgroup{subgroupMask}Mask.x";
                    }
                    else if (value == AttributeConsts.LaneId)
                    {
                        return config.GpuAccessor.QueryHostSupportsShaderBallot()
                            ? "gl_SubGroupInvocationARB"
                            : "gl_SubgroupInvocationID";
                    }

                    if (config.Stage == ShaderStage.Fragment)
                    {
                        // TODO: There must be a better way to handle this...
                        switch (value)
                        {
                            case AttributeConsts.PositionX: return $"(gl_FragCoord.x / {DefaultNames.SupportBlockRenderScaleName}[0])";
                            case AttributeConsts.PositionY: return $"(gl_FragCoord.y / {DefaultNames.SupportBlockRenderScaleName}[0])";
                            case AttributeConsts.PositionZ: return "gl_FragCoord.z";
                            case AttributeConsts.PositionW: return "gl_FragCoord.w";

                            case AttributeConsts.FrontFacing:
                                if (config.GpuAccessor.QueryHostHasFrontFacingBug())
                                {
                                    // This is required for Intel on Windows, gl_FrontFacing sometimes returns incorrect
                                    // (flipped) values. Doing this seems to fix it.
                                    return "(-floatBitsToInt(float(gl_FrontFacing)) < 0)";
                                }
                                break;
                        }
                    }

                    string name = builtInAttr.Name;

                    if (AttributeInfo.IsArrayAttributeGlsl(config.Stage, isOutAttr) && AttributeInfo.IsArrayBuiltIn(value))
                    {
                        name = isOutAttr ? $"gl_out[gl_InvocationID].{name}" : $"gl_in[{indexExpr}].{name}";
                    }

                    return name;
                }
            }

            // TODO: Warn about unknown built-in attribute.

            return isOutAttr ? "// bad_attr0x" + value.ToString("X") : "0.0";
        }

        public static string GetAttributeName(string attrExpr, ShaderConfig config, bool isOutAttr = false, string indexExpr = "0")
        {
            string name = isOutAttr
                ? DefaultNames.OAttributePrefix
                : DefaultNames.IAttributePrefix;

            if (config.Stage == ShaderStage.Geometry && !isOutAttr)
            {
                name += $"[{indexExpr}]";
            }

            return $"{name}[{attrExpr} >> 2][{attrExpr} & 3]";
        }

        public static string GetUbName(ShaderStage stage, int slot, bool cbIndexable)
        {
            if (cbIndexable)
            {
                return GetUbName(stage, NumberFormatter.FormatInt(slot, AggregateType.S32));
            }

            return $"{GetShaderStagePrefix(stage)}_{DefaultNames.UniformNamePrefix}{slot}_{DefaultNames.UniformNameSuffix}";
        }

        private static string GetUbName(ShaderStage stage, string slotExpr)
        {
            return $"{GetShaderStagePrefix(stage)}_{DefaultNames.UniformNamePrefix}[{slotExpr}].{DefaultNames.DataName}";
        }

        public static string GetSamplerName(ShaderStage stage, AstTextureOperation texOp, string indexExpr)
        {
            return GetSamplerName(stage, texOp.CbufSlot, texOp.Handle, texOp.Type.HasFlag(SamplerType.Indexed), indexExpr);
        }

        public static string GetSamplerName(ShaderStage stage, int cbufSlot, int handle, bool indexed, string indexExpr)
        {
            string suffix = cbufSlot < 0 ? $"_tcb_{handle:X}" : $"_cb{cbufSlot}_{handle:X}";

            if (indexed)
            {
                suffix += $"a[{indexExpr}]";
            }

            return GetShaderStagePrefix(stage) + "_" + DefaultNames.SamplerNamePrefix + suffix;
        }

        public static string GetImageName(ShaderStage stage, AstTextureOperation texOp, string indexExpr)
        {
            return GetImageName(stage, texOp.CbufSlot, texOp.Handle, texOp.Format, texOp.Type.HasFlag(SamplerType.Indexed), indexExpr);
        }

        public static string GetImageName(
            ShaderStage stage,
            int cbufSlot,
            int handle,
            TextureFormat format,
            bool indexed,
            string indexExpr)
        {
            string suffix = cbufSlot < 0
                ? $"_tcb_{handle:X}_{format.ToGlslFormat()}"
                : $"_cb{cbufSlot}_{handle:X}_{format.ToGlslFormat()}";

            if (indexed)
            {
                suffix += $"a[{indexExpr}]";
            }

            return GetShaderStagePrefix(stage) + "_" + DefaultNames.ImageNamePrefix + suffix;
        }

        public static string GetShaderStagePrefix(ShaderStage stage)
        {
            int index = (int)stage;

            if ((uint)index >= StagePrefixes.Length)
            {
                return "invalid";
            }

            return StagePrefixes[index];
        }

        private static char GetSwizzleMask(int value)
        {
            return "xyzw"[value];
        }

        public static string GetArgumentName(int argIndex)
        {
            return $"{DefaultNames.ArgumentNamePrefix}{argIndex}";
        }

        public static AggregateType GetNodeDestType(CodeGenContext context, IAstNode node, bool isAsgDest = false)
        {
            if (node is AstOperation operation)
            {
                if (operation.Inst == Instruction.LoadAttribute)
                {
                    // Load attribute basically just returns the attribute value.
                    // Some built-in attributes may have different types, so we need
                    // to return the type based on the attribute that is being read.
                    if (operation.GetSource(0) is AstOperand operand && operand.Type == OperandType.Constant)
                    {
                        if (_builtInAttributes.TryGetValue(operand.Value & ~3, out BuiltInAttribute builtInAttr))
                        {
                            return builtInAttr.Type;
                        }
                    }

                    return OperandInfo.GetVarType(OperandType.Attribute);
                }
                else if (operation.Inst == Instruction.Call)
                {
                    AstOperand funcId = (AstOperand)operation.GetSource(0);

                    Debug.Assert(funcId.Type == OperandType.Constant);

                    return context.GetFunction(funcId.Value).ReturnType;
                }
                else if (operation.Inst == Instruction.VectorExtract)
                {
                    return GetNodeDestType(context, operation.GetSource(0)) & ~AggregateType.ElementCountMask;
                }
                else if (operation is AstTextureOperation texOp)
                {
                    if (texOp.Inst == Instruction.ImageLoad ||
                        texOp.Inst == Instruction.ImageStore ||
                        texOp.Inst == Instruction.ImageAtomic)
                    {
                        return texOp.GetVectorType(texOp.Format.GetComponentType());
                    }
                    else if (texOp.Inst == Instruction.TextureSample)
                    {
                        return texOp.GetVectorType(GetDestVarType(operation.Inst));
                    }
                }

                return GetDestVarType(operation.Inst);
            }
            else if (node is AstOperand operand)
            {
                if (operand.Type == OperandType.Argument)
                {
                    int argIndex = operand.Value;

                    return context.CurrentFunction.GetArgumentType(argIndex);
                }

                return GetOperandVarType(context, operand, isAsgDest);
            }
            else
            {
                throw new ArgumentException($"Invalid node type \"{node?.GetType().Name ?? "null"}\".");
            }
        }

        private static AggregateType GetOperandVarType(CodeGenContext context, AstOperand operand, bool isAsgDest = false)
        {
            if (operand.Type == OperandType.Attribute)
            {
                if (_builtInAttributes.TryGetValue(operand.Value & ~3, out BuiltInAttribute builtInAttr))
                {
                    return builtInAttr.Type;
                }
                else if (context.Config.Stage == ShaderStage.Vertex && !isAsgDest &&
                    operand.Value >= AttributeConsts.UserAttributeBase &&
                    operand.Value < AttributeConsts.UserAttributeEnd)
                {
                    int location = (operand.Value - AttributeConsts.UserAttributeBase) / 16;

                    AttributeType type = context.Config.GpuAccessor.QueryAttributeType(location);

                    return type.ToAggregateType();
                }
            }

            return OperandInfo.GetVarType(operand);
        }
    }
}
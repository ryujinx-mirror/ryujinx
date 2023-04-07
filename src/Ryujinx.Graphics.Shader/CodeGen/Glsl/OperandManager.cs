using Ryujinx.Graphics.Shader.CodeGen.Glsl.Instructions;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.StructuredIr;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using static Ryujinx.Graphics.Shader.StructuredIr.InstructionInfo;

namespace Ryujinx.Graphics.Shader.CodeGen.Glsl
{
    class OperandManager
    {
        private static readonly string[] _stagePrefixes = new string[] { "cp", "vp", "tcp", "tep", "gp", "fp" };

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

            if ((uint)index >= _stagePrefixes.Length)
            {
                return "invalid";
            }

            return _stagePrefixes[index];
        }

        private static char GetSwizzleMask(int value)
        {
            return "xyzw"[value];
        }

        public static string GetArgumentName(int argIndex)
        {
            return $"{DefaultNames.ArgumentNamePrefix}{argIndex}";
        }

        public static AggregateType GetNodeDestType(CodeGenContext context, IAstNode node)
        {
            // TODO: Get rid of that function entirely and return the type from the operation generation
            // functions directly, like SPIR-V does.

            if (node is AstOperation operation)
            {
                if (operation.Inst == Instruction.Load)
                {
                    switch (operation.StorageKind)
                    {
                        case StorageKind.Input:
                        case StorageKind.InputPerPatch:
                        case StorageKind.Output:
                        case StorageKind.OutputPerPatch:
                            if (!(operation.GetSource(0) is AstOperand varId) || varId.Type != OperandType.Constant)
                            {
                                throw new InvalidOperationException($"First input of {operation.Inst} with {operation.StorageKind} storage must be a constant operand.");
                            }

                            IoVariable ioVariable = (IoVariable)varId.Value;
                            bool isOutput = operation.StorageKind == StorageKind.Output || operation.StorageKind == StorageKind.OutputPerPatch;
                            bool isPerPatch = operation.StorageKind == StorageKind.InputPerPatch || operation.StorageKind == StorageKind.OutputPerPatch;
                            int location = 0;
                            int component = 0;

                            if (context.Config.HasPerLocationInputOrOutput(ioVariable, isOutput))
                            {
                                if (!(operation.GetSource(1) is AstOperand vecIndex) || vecIndex.Type != OperandType.Constant)
                                {
                                    throw new InvalidOperationException($"Second input of {operation.Inst} with {operation.StorageKind} storage must be a constant operand.");
                                }

                                location = vecIndex.Value;

                                if (operation.SourcesCount > 2 &&
                                    operation.GetSource(2) is AstOperand elemIndex &&
                                    elemIndex.Type == OperandType.Constant &&
                                    context.Config.HasPerLocationInputOrOutputComponent(ioVariable, location, elemIndex.Value, isOutput))
                                {
                                    component = elemIndex.Value;
                                }
                            }

                            (_, AggregateType varType) = IoMap.GetGlslVariable(context.Config, ioVariable, location, component, isOutput, isPerPatch);

                            return varType & AggregateType.ElementTypeMask;
                    }
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

                return OperandInfo.GetVarType(operand);
            }
            else
            {
                throw new ArgumentException($"Invalid node type \"{node?.GetType().Name ?? "null"}\".");
            }
        }
    }
}
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Translation
{
    static class ShaderIdentifier
    {
        public static ShaderIdentification Identify(IReadOnlyList<Function> functions, ShaderConfig config)
        {
            if (config.Stage == ShaderStage.Geometry &&
                config.GpuAccessor.QueryPrimitiveTopology() == InputTopology.Triangles &&
                !config.GpuAccessor.QueryHostSupportsGeometryShader() &&
                IsLayerPassthroughGeometryShader(functions, out int layerInputAttr))
            {
                config.SetGeometryShaderLayerInputAttribute(layerInputAttr);

                return ShaderIdentification.GeometryLayerPassthrough;
            }

            return ShaderIdentification.None;
        }

        private static bool IsLayerPassthroughGeometryShader(IReadOnlyList<Function> functions, out int layerInputAttr)
        {
            bool writesLayer = false;
            layerInputAttr = 0;

            if (functions.Count != 1)
            {
                return false;
            }

            int verticesCount = 0;
            int totalVerticesCount = 0;

            foreach (BasicBlock block in functions[0].Blocks)
            {
                // We are not expecting loops or any complex control flow here, so fail in those cases.
                if (block.Branch != null && block.Branch.Index <= block.Index)
                {
                    return false;
                }

                foreach (INode node in block.Operations)
                {
                    if (node is not Operation operation)
                    {
                        continue;
                    }

                    if (IsResourceWrite(operation.Inst, operation.StorageKind))
                    {
                        return false;
                    }

                    if (operation.Inst == Instruction.Store && operation.StorageKind == StorageKind.Output)
                    {
                        Operand src = operation.GetSource(operation.SourcesCount - 1);
                        Operation srcAttributeAsgOp = null;

                        if (src.Type == OperandType.LocalVariable &&
                            src.AsgOp is Operation asgOp &&
                            asgOp.Inst == Instruction.Load &&
                            asgOp.StorageKind.IsInputOrOutput())
                        {
                            if (asgOp.StorageKind != StorageKind.Input)
                            {
                                return false;
                            }

                            srcAttributeAsgOp = asgOp;
                        }

                        if (srcAttributeAsgOp != null)
                        {
                            IoVariable dstAttribute = (IoVariable)operation.GetSource(0).Value;
                            IoVariable srcAttribute = (IoVariable)srcAttributeAsgOp.GetSource(0).Value;

                            if (dstAttribute == IoVariable.Layer && srcAttribute == IoVariable.UserDefined)
                            {
                                if (srcAttributeAsgOp.SourcesCount != 4)
                                {
                                    return false;
                                }

                                writesLayer = true;
                                layerInputAttr = srcAttributeAsgOp.GetSource(1).Value * 4 + srcAttributeAsgOp.GetSource(3).Value;
                            }
                            else
                            {
                                if (dstAttribute != srcAttribute)
                                {
                                    return false;
                                }

                                int inputsCount = operation.SourcesCount - 2;

                                if (dstAttribute == IoVariable.UserDefined)
                                {
                                    if (operation.GetSource(1).Value != srcAttributeAsgOp.GetSource(1).Value)
                                    {
                                        return false;
                                    }

                                    inputsCount--;
                                }

                                for (int i = 0; i < inputsCount; i++)
                                {
                                    int dstIndex = operation.SourcesCount - 2 - i;
                                    int srcIndex = srcAttributeAsgOp.SourcesCount - 1 - i;

                                    if ((dstIndex | srcIndex) < 0)
                                    {
                                        return false;
                                    }

                                    if (operation.GetSource(dstIndex).Type != OperandType.Constant ||
                                        srcAttributeAsgOp.GetSource(srcIndex).Type != OperandType.Constant ||
                                        operation.GetSource(dstIndex).Value != srcAttributeAsgOp.GetSource(srcIndex).Value)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                        else if (src.Type == OperandType.Constant)
                        {
                            int dstComponent = operation.GetSource(operation.SourcesCount - 2).Value;
                            float expectedValue = dstComponent == 3 ? 1f : 0f;

                            if (src.AsFloat() != expectedValue)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (operation.Inst == Instruction.EmitVertex)
                    {
                        verticesCount++;
                    }
                    else if (operation.Inst == Instruction.EndPrimitive)
                    {
                        totalVerticesCount += verticesCount;
                        verticesCount = 0;
                    }
                }
            }

            return totalVerticesCount + verticesCount == 3 && writesLayer;
        }

        private static bool IsResourceWrite(Instruction inst, StorageKind storageKind)
        {
            switch (inst)
            {
                case Instruction.AtomicAdd:
                case Instruction.AtomicAnd:
                case Instruction.AtomicCompareAndSwap:
                case Instruction.AtomicMaxS32:
                case Instruction.AtomicMaxU32:
                case Instruction.AtomicMinS32:
                case Instruction.AtomicMinU32:
                case Instruction.AtomicOr:
                case Instruction.AtomicSwap:
                case Instruction.AtomicXor:
                case Instruction.ImageAtomic:
                case Instruction.ImageStore:
                    return true;
                case Instruction.Store:
                    return storageKind == StorageKind.StorageBuffer ||
                           storageKind == StorageKind.SharedMemory ||
                           storageKind == StorageKind.LocalMemory;
            }

            return false;
        }
    }
}

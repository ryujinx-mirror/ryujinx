using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Translation
{
    static class ShaderIdentifier
    {
        public static ShaderIdentification Identify(Function[] functions, ShaderConfig config)
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

        private static bool IsLayerPassthroughGeometryShader(Function[] functions, out int layerInputAttr)
        {
            bool writesLayer = false;
            layerInputAttr = 0;

            if (functions.Length != 1)
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
                    if (!(node is Operation operation))
                    {
                        continue;
                    }

                    if (IsResourceWrite(operation.Inst))
                    {
                        return false;
                    }

                    if (operation.Inst == Instruction.StoreAttribute)
                    {
                        return false;
                    }

                    if (operation.Inst == Instruction.Copy && operation.Dest.Type == OperandType.Attribute)
                    {
                        Operand src = operation.GetSource(0);

                        if (src.Type == OperandType.LocalVariable && src.AsgOp is Operation asgOp && asgOp.Inst == Instruction.LoadAttribute)
                        {
                            src = Attribute(asgOp.GetSource(0).Value);
                        }

                        if (src.Type == OperandType.Attribute)
                        {
                            if (operation.Dest.Value == AttributeConsts.Layer)
                            {
                                if ((src.Value & AttributeConsts.LoadOutputMask) != 0)
                                {
                                    return false;
                                }

                                writesLayer = true;
                                layerInputAttr = src.Value;
                            }
                            else if (src.Value != operation.Dest.Value)
                            {
                                return false;
                            }
                        }
                        else if (src.Type == OperandType.Constant)
                        {
                            int dstComponent = (operation.Dest.Value >> 2) & 3;
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

        private static bool IsResourceWrite(Instruction inst)
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
                case Instruction.StoreGlobal:
                case Instruction.StoreGlobal16:
                case Instruction.StoreGlobal8:
                case Instruction.StoreStorage:
                case Instruction.StoreStorage16:
                case Instruction.StoreStorage8:
                    return true;
            }

            return false;
        }
    }
}

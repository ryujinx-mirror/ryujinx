using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Translation.Transforms
{
    class DrawParametersReplace : ITransformPass
    {
        public static bool IsEnabled(IGpuAccessor gpuAccessor, ShaderStage stage, TargetLanguage targetLanguage, FeatureFlags usedFeatures)
        {
            return stage == ShaderStage.Vertex;
        }

        public static LinkedListNode<INode> RunPass(TransformContext context, LinkedListNode<INode> node)
        {
            Operation operation = (Operation)node.Value;

            if (context.GpuAccessor.QueryHasConstantBufferDrawParameters())
            {
                if (ReplaceConstantBufferWithDrawParameters(node, operation))
                {
                    context.UsedFeatures |= FeatureFlags.DrawParameters;
                }
            }
            else if (HasConstantBufferDrawParameters(operation))
            {
                context.UsedFeatures |= FeatureFlags.DrawParameters;
            }

            return node;
        }

        private static bool ReplaceConstantBufferWithDrawParameters(LinkedListNode<INode> node, Operation operation)
        {
            Operand GenerateLoad(IoVariable ioVariable)
            {
                Operand value = Local();
                node.List.AddBefore(node, new Operation(Instruction.Load, StorageKind.Input, value, Const((int)ioVariable)));
                return value;
            }

            bool modified = false;

            for (int srcIndex = 0; srcIndex < operation.SourcesCount; srcIndex++)
            {
                Operand src = operation.GetSource(srcIndex);

                if (src.Type == OperandType.ConstantBuffer && src.GetCbufSlot() == 0)
                {
                    switch (src.GetCbufOffset())
                    {
                        case Constants.NvnBaseVertexByteOffset / 4:
                            operation.SetSource(srcIndex, GenerateLoad(IoVariable.BaseVertex));
                            modified = true;
                            break;
                        case Constants.NvnBaseInstanceByteOffset / 4:
                            operation.SetSource(srcIndex, GenerateLoad(IoVariable.BaseInstance));
                            modified = true;
                            break;
                        case Constants.NvnDrawIndexByteOffset / 4:
                            operation.SetSource(srcIndex, GenerateLoad(IoVariable.DrawIndex));
                            modified = true;
                            break;
                    }
                }
            }

            return modified;
        }

        private static bool HasConstantBufferDrawParameters(Operation operation)
        {
            for (int srcIndex = 0; srcIndex < operation.SourcesCount; srcIndex++)
            {
                Operand src = operation.GetSource(srcIndex);

                if (src.Type == OperandType.ConstantBuffer && src.GetCbufSlot() == 0)
                {
                    switch (src.GetCbufOffset())
                    {
                        case Constants.NvnBaseVertexByteOffset / 4:
                        case Constants.NvnBaseInstanceByteOffset / 4:
                        case Constants.NvnDrawIndexByteOffset / 4:
                            return true;
                    }
                }
            }

            return false;
        }
    }
}

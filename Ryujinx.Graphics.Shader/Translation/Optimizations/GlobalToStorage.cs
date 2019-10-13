using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Translation.Optimizations
{
    static class GlobalToStorage
    {
        private const int StorageDescsBaseOffset = 0x44; // In words.

        private const int UbeStorageDescsBaseOffset = 0x84; // In words.
        private const int UbeStorageMaxCount        = 14;

        private const int StorageDescSize = 4; // In words.
        private const int StorageMaxCount = 16;

        private const int StorageDescsSize  = StorageDescSize * StorageMaxCount;

        public static void RunPass(BasicBlock block, ShaderStage stage)
        {
            int sbStart = GetStorageBaseCbOffset(stage);

            int sbEnd = sbStart + StorageDescsSize;

            // This one is only used on compute shaders.
            // Compute shaders uses two separate sets of storage.
            int ubeSbStart = UbeStorageDescsBaseOffset;
            int ubeSbEnd   = UbeStorageDescsBaseOffset + StorageDescSize * UbeStorageMaxCount;

            for (LinkedListNode<INode> node = block.Operations.First; node != null; node = node.Next)
            {
                if (!(node.Value is Operation operation))
                {
                    continue;
                }

                if (operation.Inst == Instruction.LoadGlobal ||
                    operation.Inst == Instruction.StoreGlobal)
                {
                    Operand source = operation.GetSource(0);

                    if (source.AsgOp is Operation asgOperation)
                    {
                        int storageIndex = SearchForStorageBase(asgOperation, sbStart, sbEnd);

                        /*if (storageIndex < 0 && stage == ShaderStage.Compute)
                        {
                            storageIndex = SearchForStorageBase(asgOperation, ubeSbStart, ubeSbEnd);
                        }*/

                        if (storageIndex >= 0)
                        {
                            node = ReplaceGlobalWithStorage(node, storageIndex);
                        }
                    }
                }
            }
        }

        private static LinkedListNode<INode> ReplaceGlobalWithStorage(LinkedListNode<INode> node, int storageIndex)
        {
            Operation operation = (Operation)node.Value;

            Operation storageOp;

            if (operation.Inst == Instruction.LoadGlobal)
            {
                Operand source = operation.GetSource(0);

                storageOp = new Operation(Instruction.LoadStorage, operation.Dest, Const(storageIndex), source);
            }
            else
            {
                Operand src1 = operation.GetSource(0);
                Operand src2 = operation.GetSource(1);

                storageOp = new Operation(Instruction.StoreStorage, null, Const(storageIndex), src1, src2);
            }

            for (int index = 0; index < operation.SourcesCount; index++)
            {
                operation.SetSource(index, null);
            }

            LinkedListNode<INode> oldNode = node;

            node = node.List.AddAfter(node, storageOp);

            node.List.Remove(oldNode);

            return node;
        }

        private static int SearchForStorageBase(Operation operation, int sbStart, int sbEnd)
        {
            Queue<Operation> assignments = new Queue<Operation>();

            assignments.Enqueue(operation);

            while (assignments.TryDequeue(out operation))
            {
                for (int index = 0; index < operation.SourcesCount; index++)
                {
                    Operand source = operation.GetSource(index);

                    if (source.Type == OperandType.ConstantBuffer)
                    {
                        int slot   = source.GetCbufSlot();
                        int offset = source.GetCbufOffset();

                        if (slot == 0 && offset >= sbStart && offset < sbEnd)
                        {
                            int storageIndex = (offset - sbStart) / StorageDescSize;

                            return storageIndex;
                        }
                    }

                    if (source.AsgOp is Operation asgOperation)
                    {
                        assignments.Enqueue(asgOperation);
                    }
                }
            }

            return -1;
        }

        private static int GetStorageBaseCbOffset(ShaderStage stage)
        {
            switch (stage)
            {
                case ShaderStage.Compute:                return StorageDescsBaseOffset + 2 * StorageDescsSize;
                case ShaderStage.Vertex:                 return StorageDescsBaseOffset;
                case ShaderStage.TessellationControl:    return StorageDescsBaseOffset + 1 * StorageDescsSize;
                case ShaderStage.TessellationEvaluation: return StorageDescsBaseOffset + 2 * StorageDescsSize;
                case ShaderStage.Geometry:               return StorageDescsBaseOffset + 3 * StorageDescsSize;
                case ShaderStage.Fragment:               return StorageDescsBaseOffset + 4 * StorageDescsSize;
            }

            return 0;
        }
    }
}
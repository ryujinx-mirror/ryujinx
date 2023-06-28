using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Translation.Optimizations
{
    static class Utils
    {
        public static bool IsInputLoad(INode node)
        {
            return (node is Operation operation) &&
                   operation.Inst == Instruction.Load &&
                   operation.StorageKind == StorageKind.Input;
        }

        public static bool IsInputLoad(INode node, IoVariable ioVariable, int elemIndex)
        {
            if (node is not Operation operation ||
                operation.Inst != Instruction.Load ||
                operation.StorageKind != StorageKind.Input ||
                operation.SourcesCount != 2)
            {
                return false;
            }

            Operand ioVariableSrc = operation.GetSource(0);

            if (ioVariableSrc.Type != OperandType.Constant || (IoVariable)ioVariableSrc.Value != ioVariable)
            {
                return false;
            }

            Operand elemIndexSrc = operation.GetSource(1);

            return elemIndexSrc.Type == OperandType.Constant && elemIndexSrc.Value == elemIndex;
        }

        private static Operation FindBranchSource(BasicBlock block)
        {
            foreach (BasicBlock sourceBlock in block.Predecessors)
            {
                if (sourceBlock.Operations.Count > 0)
                {
                    if (sourceBlock.GetLastOp() is Operation lastOp && IsConditionalBranch(lastOp.Inst) && sourceBlock.Next == block)
                    {
                        return lastOp;
                    }
                }
            }

            return null;
        }

        private static bool IsConditionalBranch(Instruction inst)
        {
            return inst == Instruction.BranchIfFalse || inst == Instruction.BranchIfTrue;
        }

        private static bool BlockConditionsMatch(BasicBlock currentBlock, BasicBlock queryBlock)
        {
            // Check if all the conditions for the query block are satisfied by the current block.
            // Just checks the top-most conditional for now.

            Operation currentBranch = FindBranchSource(currentBlock);
            Operation queryBranch = FindBranchSource(queryBlock);

            Operand currentCondition = currentBranch?.GetSource(0);
            Operand queryCondition = queryBranch?.GetSource(0);

            // The condition should be the same operand instance.

            return currentBranch != null && queryBranch != null &&
                   currentBranch.Inst == queryBranch.Inst &&
                   currentCondition == queryCondition;
        }

        public static Operand FindLastOperation(Operand source, BasicBlock block)
        {
            if (source.AsgOp is PhiNode phiNode)
            {
                // This source can have a different value depending on a previous branch.
                // Ensure that conditions met for that branch are also met for the current one.
                // Prefer the latest sources for the phi node.

                for (int i = phiNode.SourcesCount - 1; i >= 0; i--)
                {
                    BasicBlock phiBlock = phiNode.GetBlock(i);

                    if (BlockConditionsMatch(block, phiBlock))
                    {
                        return phiNode.GetSource(i);
                    }
                }
            }

            return source;
        }

        public static void DeleteNode(LinkedListNode<INode> node, Operation operation)
        {
            node.List.Remove(node);

            for (int srcIndex = 0; srcIndex < operation.SourcesCount; srcIndex++)
            {
                operation.SetSource(srcIndex, null);
            }

            operation.Dest = null;
        }
    }
}

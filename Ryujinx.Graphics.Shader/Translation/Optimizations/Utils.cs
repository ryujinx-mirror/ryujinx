using Ryujinx.Graphics.Shader.IntermediateRepresentation;

namespace Ryujinx.Graphics.Shader.Translation.Optimizations
{
    static class Utils
    {
        private static Operation FindBranchSource(BasicBlock block)
        {
            foreach (BasicBlock sourceBlock in block.Predecessors)
            {
                if (sourceBlock.Operations.Count > 0)
                {
                    Operation lastOp = sourceBlock.Operations.Last.Value as Operation;

                    if (lastOp != null &&
                        ((sourceBlock.Next == block && lastOp.Inst == Instruction.BranchIfFalse) ||
                        (sourceBlock.Branch == block && lastOp.Inst == Instruction.BranchIfTrue)))
                    {
                        return lastOp;
                    }
                }
            }

            return null;
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
    }
}

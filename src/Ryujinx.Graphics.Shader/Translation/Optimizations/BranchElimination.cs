using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System;

namespace Ryujinx.Graphics.Shader.Translation.Optimizations
{
    static class BranchElimination
    {
        public static bool RunPass(BasicBlock block)
        {
            if (block.HasBranch && IsRedundantBranch((Operation)block.GetLastOp(), Next(block)))
            {
                block.Branch = null;

                return true;
            }

            return false;
        }

        private static bool IsRedundantBranch(Operation current, BasicBlock nextBlock)
        {
            // Here we check that:
            // - The current block ends with a branch.
            // - The next block only contains a branch.
            // - The branch on the next block is unconditional.
            // - Both branches are jumping to the same location.
            // In this case, the branch on the current block can be removed,
            // as the next block is going to jump to the same place anyway.
            if (nextBlock == null)
            {
                return false;
            }

            if (nextBlock.Operations.First?.Value is not Operation next)
            {
                return false;
            }

            if (next.Inst != Instruction.Branch)
            {
                return false;
            }

            return current.Dest == next.Dest;
        }

        private static BasicBlock Next(BasicBlock block)
        {
            block = block.Next;

            while (block != null && block.Operations.Count == 0)
            {
                if (block.HasBranch)
                {
                    throw new InvalidOperationException("Found a bogus empty block that \"ends with a branch\".");
                }

                block = block.Next;
            }

            return block;
        }
    }
}

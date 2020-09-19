using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System.Diagnostics;

using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.CodeGen.Optimizations
{
    static class BlockPlacement
    {
        public static void RunPass(ControlFlowGraph cfg)
        {
            bool update = false;

            BasicBlock block;
            BasicBlock nextBlock;

            BasicBlock lastBlock = cfg.Blocks.Last;

            // Move cold blocks at the end of the list, so that they are emitted away from hot code.
            for (block = cfg.Blocks.First; block != lastBlock; block = nextBlock)
            {
                nextBlock = block.ListNext;

                if (block.Frequency == BasicBlockFrequency.Cold)
                {
                    cfg.Blocks.Remove(block);
                    cfg.Blocks.AddLast(block);
                }
            }

            for (block = cfg.Blocks.First; block != null; block = nextBlock)
            {
                nextBlock = block.ListNext;

                if (block.SuccessorCount == 2 && block.Operations.Last is Operation branchOp)
                {
                    Debug.Assert(branchOp.Instruction == Instruction.BranchIf);

                    BasicBlock falseSucc = block.GetSuccessor(0);
                    BasicBlock trueSucc = block.GetSuccessor(1);

                    // If true successor is next block in list, invert the condition. We avoid extra branching by
                    // making the true side the fallthrough (i.e, convert it to the false side).
                    if (trueSucc == block.ListNext)
                    {
                        Comparison comp = (Comparison)branchOp.GetSource(2).AsInt32();
                        Comparison compInv = comp.Invert();

                        branchOp.SetSource(2, Const((int)compInv));

                        block.SetSuccessor(0, trueSucc);
                        block.SetSuccessor(1, falseSucc);

                        update = true;
                    }
                }
            }

            if (update)
            {
                cfg.Update(removeUnreachableBlocks: false);
            }
        }
    }
}

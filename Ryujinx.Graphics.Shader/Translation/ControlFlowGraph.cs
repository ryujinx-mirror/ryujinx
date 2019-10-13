using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Translation
{
    static class ControlFlowGraph
    {
        public static BasicBlock[] MakeCfg(Operation[] operations)
        {
            Dictionary<Operand, BasicBlock> labels = new Dictionary<Operand, BasicBlock>();

            List<BasicBlock> blocks = new List<BasicBlock>();

            BasicBlock currentBlock = null;

            void NextBlock(BasicBlock nextBlock)
            {
                if (currentBlock != null && !EndsWithUnconditionalInst(currentBlock.GetLastOp()))
                {
                    currentBlock.Next = nextBlock;
                }

                currentBlock = nextBlock;
            }

            void NewNextBlock()
            {
                BasicBlock block = new BasicBlock(blocks.Count);

                blocks.Add(block);

                NextBlock(block);
            }

            bool needsNewBlock = true;

            for (int index = 0; index < operations.Length; index++)
            {
                Operation operation = operations[index];

                if (operation.Inst == Instruction.MarkLabel)
                {
                    Operand label = operation.Dest;

                    if (labels.TryGetValue(label, out BasicBlock nextBlock))
                    {
                        nextBlock.Index = blocks.Count;

                        blocks.Add(nextBlock);

                        NextBlock(nextBlock);
                    }
                    else
                    {
                        NewNextBlock();

                        labels.Add(label, currentBlock);
                    }
                }
                else
                {
                    if (needsNewBlock)
                    {
                        NewNextBlock();
                    }

                    currentBlock.Operations.AddLast(operation);
                }

                needsNewBlock = operation.Inst == Instruction.Branch       ||
                                operation.Inst == Instruction.BranchIfTrue ||
                                operation.Inst == Instruction.BranchIfFalse;

                if (needsNewBlock)
                {
                    Operand label = operation.Dest;

                    if (!labels.TryGetValue(label, out BasicBlock branchBlock))
                    {
                        branchBlock = new BasicBlock();

                        labels.Add(label, branchBlock);
                    }

                    currentBlock.Branch = branchBlock;
                }
            }

            return blocks.ToArray();
        }

        private static bool EndsWithUnconditionalInst(INode node)
        {
            if (node is Operation operation)
            {
                switch (operation.Inst)
                {
                    case Instruction.Branch:
                    case Instruction.Discard:
                    case Instruction.Return:
                        return true;
                }
            }

            return false;
        }
    }
}
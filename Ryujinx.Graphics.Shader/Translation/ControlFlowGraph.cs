using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Translation
{
    class ControlFlowGraph
    {
        public BasicBlock[] Blocks { get; }
        public BasicBlock[] PostOrderBlocks { get; }
        public int[] PostOrderMap { get; }

        public ControlFlowGraph(BasicBlock[] blocks)
        {
            Blocks = blocks;

            HashSet<BasicBlock> visited = new HashSet<BasicBlock>();

            Stack<BasicBlock> blockStack = new Stack<BasicBlock>();

            List<BasicBlock> postOrderBlocks = new List<BasicBlock>(blocks.Length);

            PostOrderMap = new int[blocks.Length];

            visited.Add(blocks[0]);

            blockStack.Push(blocks[0]);

            while (blockStack.TryPop(out BasicBlock block))
            {
                if (block.Next != null && visited.Add(block.Next))
                {
                    blockStack.Push(block);
                    blockStack.Push(block.Next);
                }
                else if (block.Branch != null && visited.Add(block.Branch))
                {
                    blockStack.Push(block);
                    blockStack.Push(block.Branch);
                }
                else
                {
                    PostOrderMap[block.Index] = postOrderBlocks.Count;

                    postOrderBlocks.Add(block);
                }
            }

            PostOrderBlocks = postOrderBlocks.ToArray();
        }

        public static ControlFlowGraph Create(Operation[] operations)
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

            return new ControlFlowGraph(blocks.ToArray());
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
using ARMeilleure.IntermediateRepresentation;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ARMeilleure.Translation
{
    class ControlFlowGraph
    {
        public BasicBlock Entry { get; }

        public IntrusiveList<BasicBlock> Blocks { get; }

        public BasicBlock[] PostOrderBlocks { get; }

        public int[] PostOrderMap { get; }

        public ControlFlowGraph(BasicBlock entry, IntrusiveList<BasicBlock> blocks)
        {
            Entry  = entry;
            Blocks = blocks;

            RemoveUnreachableBlocks(blocks);

            HashSet<BasicBlock> visited = new HashSet<BasicBlock>();

            Stack<BasicBlock> blockStack = new Stack<BasicBlock>();

            PostOrderBlocks = new BasicBlock[blocks.Count];

            PostOrderMap = new int[blocks.Count];

            visited.Add(entry);

            blockStack.Push(entry);

            int index = 0;

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
                    PostOrderMap[block.Index] = index;

                    PostOrderBlocks[index++] = block;
                }
            }
        }

        private void RemoveUnreachableBlocks(IntrusiveList<BasicBlock> blocks)
        {
            HashSet<BasicBlock> visited = new HashSet<BasicBlock>();

            Queue<BasicBlock> workQueue = new Queue<BasicBlock>();

            visited.Add(Entry);

            workQueue.Enqueue(Entry);

            while (workQueue.TryDequeue(out BasicBlock block))
            {
                Debug.Assert(block.Index != -1, "Invalid block index.");

                if (block.Next != null && visited.Add(block.Next))
                {
                    workQueue.Enqueue(block.Next);
                }

                if (block.Branch != null && visited.Add(block.Branch))
                {
                    workQueue.Enqueue(block.Branch);
                }
            }

            if (visited.Count < blocks.Count)
            {
                // Remove unreachable blocks and renumber.
                int index = 0;

                for (BasicBlock block = blocks.First; block != null;)
                {
                    BasicBlock nextBlock = block.ListNext;

                    if (!visited.Contains(block))
                    {
                        block.Next = null;
                        block.Branch = null;

                        blocks.Remove(block);
                    }
                    else
                    {
                        block.Index = index++;
                    }

                    block = nextBlock;
                }
            }
        }

        public BasicBlock SplitEdge(BasicBlock predecessor, BasicBlock successor)
        {
            BasicBlock splitBlock = new BasicBlock(Blocks.Count);

            if (predecessor.Next == successor)
            {
                predecessor.Next = splitBlock;
            }

            if (predecessor.Branch == successor)
            {
                predecessor.Branch = splitBlock;
            }

            if (splitBlock.Predecessors.Count == 0)
            {
                throw new ArgumentException("Predecessor and successor are not connected.");
            }

            // Insert the new block on the list of blocks.
            BasicBlock succPrev = successor.ListPrevious;

            if (succPrev != null && succPrev != predecessor && succPrev.Next == successor)
            {
                // Can't insert after the predecessor or before the successor.
                // Here, we insert it before the successor by also spliting another
                // edge (the one between the block before "successor" and "successor").
                BasicBlock splitBlock2 = new BasicBlock(splitBlock.Index + 1);

                succPrev.Next = splitBlock2;

                splitBlock2.Branch = successor;

                splitBlock2.Operations.AddLast(new Operation(Instruction.Branch, null));

                Blocks.AddBefore(successor, splitBlock2);
            }

            splitBlock.Next = successor;

            Blocks.AddBefore(successor, splitBlock);

            return splitBlock;
        }
    }
}
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Translation
{
    static class Dominance
    {
        // Those methods are an implementation of the algorithms on "A Simple, Fast Dominance Algorithm".
        // https://www.cs.rice.edu/~keith/EMBED/dom.pdf
        public static void FindDominators(BasicBlock entry, int blocksCount)
        {
            HashSet<BasicBlock> visited = new HashSet<BasicBlock>();

            Stack<BasicBlock> blockStack = new Stack<BasicBlock>();

            List<BasicBlock> postOrderBlocks = new List<BasicBlock>(blocksCount);

            int[] postOrderMap = new int[blocksCount];

            visited.Add(entry);

            blockStack.Push(entry);

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
                    postOrderMap[block.Index] = postOrderBlocks.Count;

                    postOrderBlocks.Add(block);
                }
            }

            BasicBlock Intersect(BasicBlock block1, BasicBlock block2)
            {
                while (block1 != block2)
                {
                    while (postOrderMap[block1.Index] < postOrderMap[block2.Index])
                    {
                        block1 = block1.ImmediateDominator;
                    }

                    while (postOrderMap[block2.Index] < postOrderMap[block1.Index])
                    {
                        block2 = block2.ImmediateDominator;
                    }
                }

                return block1;
            }

            entry.ImmediateDominator = entry;

            bool modified;

            do
            {
                modified = false;

                for (int blkIndex = postOrderBlocks.Count - 2; blkIndex >= 0; blkIndex--)
                {
                    BasicBlock block = postOrderBlocks[blkIndex];

                    BasicBlock newIDom = null;

                    foreach (BasicBlock predecessor in block.Predecessors)
                    {
                        if (predecessor.ImmediateDominator != null)
                        {
                            if (newIDom != null)
                            {
                                newIDom = Intersect(predecessor, newIDom);
                            }
                            else
                            {
                                newIDom = predecessor;
                            }
                        }
                    }

                    if (block.ImmediateDominator != newIDom)
                    {
                        block.ImmediateDominator = newIDom;

                        modified = true;
                    }
                }
            }
            while (modified);
        }

        public static void FindDominanceFrontiers(BasicBlock[] blocks)
        {
            for (int blkIndex = 0; blkIndex < blocks.Length; blkIndex++)
            {
                BasicBlock block = blocks[blkIndex];

                if (block.Predecessors.Count < 2)
                {
                    continue;
                }

                for (int pBlkIndex = 0; pBlkIndex < block.Predecessors.Count; pBlkIndex++)
                {
                    BasicBlock current = block.Predecessors[pBlkIndex];

                    while (current != block.ImmediateDominator)
                    {
                        current.DominanceFrontiers.Add(block);

                        current = current.ImmediateDominator;
                    }
                }
            }
        }
    }
}
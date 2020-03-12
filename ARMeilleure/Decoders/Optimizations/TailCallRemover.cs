using ARMeilleure.Decoders;
using System;
using System.Collections.Generic;

namespace ARMeilleure.Decoders.Optimizations
{
    static class TailCallRemover
    {
        public static void RunPass(ulong entryAddress, List<Block> blocks)
        {
            // Detect tail calls:
            // - Assume this function spans the space covered by contiguous code blocks surrounding the entry address.
            // - Unconditional jump to an area outside this contiguous region will be treated as a tail call.
            // - Include a small allowance for jumps outside the contiguous range.

            if (!Decoder.BinarySearch(blocks, entryAddress, out int entryBlockId))
            {
                throw new InvalidOperationException("Function entry point is not contained in a block.");
            }

            const ulong allowance = 4;
            Block entryBlock = blocks[entryBlockId];
            int startBlockIndex = entryBlockId;
            Block startBlock = entryBlock;
            int endBlockIndex = entryBlockId;
            Block endBlock = entryBlock;

            for (int i = entryBlockId + 1; i < blocks.Count; i++) // Search forwards.
            {
                Block block = blocks[i];
                if (endBlock.EndAddress < block.Address - allowance)
                {
                    break; // End of contiguous function.
                }

                endBlock = block;
                endBlockIndex = i;
            }

            for (int i = entryBlockId - 1; i >= 0; i--) // Search backwards.
            {
                Block block = blocks[i];
                if (startBlock.Address > block.EndAddress + allowance)
                {
                    break; // End of contiguous function.
                }

                startBlock = block;
                startBlockIndex = i;
            }

            if (startBlockIndex == 0 && endBlockIndex == blocks.Count - 1)
            {
                return; // Nothing to do here.
            }

            // Replace all branches to blocks outside the range with null, and force a tail call.

            for (int i = startBlockIndex; i <= endBlockIndex; i++)
            {
                Block block = blocks[i];
                if (block.Branch != null && (block.Branch.Address > endBlock.EndAddress || block.Branch.EndAddress < startBlock.Address))
                {
                    block.Branch = null;
                    block.TailCall = true;
                }
            }

            // Finally, delete all blocks outside the contiguous range.

            blocks.RemoveRange(endBlockIndex + 1, (blocks.Count - endBlockIndex) - 1);
            blocks.RemoveRange(0, startBlockIndex);
        }
    }
}

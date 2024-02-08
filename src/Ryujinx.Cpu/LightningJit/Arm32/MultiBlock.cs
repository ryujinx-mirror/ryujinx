using System.Collections.Generic;

namespace Ryujinx.Cpu.LightningJit.Arm32
{
    class MultiBlock
    {
        public readonly List<Block> Blocks;
        public readonly bool HasHostCall;
        public readonly bool HasHostCallSkipContext;
        public readonly bool IsTruncated;

        public MultiBlock(List<Block> blocks)
        {
            Blocks = blocks;

            Block block = blocks[0];

            HasHostCall = block.HasHostCall;
            HasHostCallSkipContext = block.HasHostCallSkipContext;

            for (int index = 1; index < blocks.Count; index++)
            {
                block = blocks[index];

                HasHostCall |= block.HasHostCall;
                HasHostCallSkipContext |= block.HasHostCallSkipContext;
            }

            block = blocks[^1];

            IsTruncated = block.IsTruncated;
        }
    }
}

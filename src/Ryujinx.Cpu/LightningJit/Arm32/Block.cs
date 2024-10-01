using System.Collections.Generic;
using System.Diagnostics;

namespace Ryujinx.Cpu.LightningJit.Arm32
{
    class Block
    {
        public readonly ulong Address;
        public readonly ulong EndAddress;
        public readonly List<InstInfo> Instructions;
        public readonly bool EndsWithBranch;
        public readonly bool HasHostCall;
        public readonly bool HasHostCallSkipContext;
        public readonly bool IsTruncated;
        public readonly bool IsLoopEnd;
        public readonly bool IsThumb;

        public Block(
            ulong address,
            ulong endAddress,
            List<InstInfo> instructions,
            bool endsWithBranch,
            bool hasHostCall,
            bool hasHostCallSkipContext,
            bool isTruncated,
            bool isLoopEnd,
            bool isThumb)
        {
            Debug.Assert(isThumb || (int)((endAddress - address) / 4) == instructions.Count);

            Address = address;
            EndAddress = endAddress;
            Instructions = instructions;
            EndsWithBranch = endsWithBranch;
            HasHostCall = hasHostCall;
            HasHostCallSkipContext = hasHostCallSkipContext;
            IsTruncated = isTruncated;
            IsLoopEnd = isLoopEnd;
            IsThumb = isThumb;
        }

        public (Block, Block) SplitAtAddress(ulong address)
        {
            int splitIndex = FindSplitIndex(address);

            if (splitIndex < 0)
            {
                return (null, null);
            }

            int splitCount = Instructions.Count - splitIndex;

            // Technically those are valid, but we don't want to create empty blocks.
            Debug.Assert(splitIndex != 0);
            Debug.Assert(splitCount != 0);

            Block leftBlock = new(
                Address,
                address,
                Instructions.GetRange(0, splitIndex),
                false,
                HasHostCall,
                HasHostCallSkipContext,
                false,
                false,
                IsThumb);

            Block rightBlock = new(
                address,
                EndAddress,
                Instructions.GetRange(splitIndex, splitCount),
                EndsWithBranch,
                HasHostCall,
                HasHostCallSkipContext,
                IsTruncated,
                IsLoopEnd,
                IsThumb);

            return (leftBlock, rightBlock);
        }

        private int FindSplitIndex(ulong address)
        {
            if (IsThumb)
            {
                ulong pc = Address;

                for (int index = 0; index < Instructions.Count; index++)
                {
                    if (pc == address)
                    {
                        return index;
                    }

                    pc += Instructions[index].Flags.HasFlag(InstFlags.Thumb16) ? 2UL : 4UL;
                }

                return -1;
            }
            else
            {
                return (int)((address - Address) / 4);
            }
        }
    }
}

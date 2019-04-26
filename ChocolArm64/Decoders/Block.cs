using System;
using System.Collections.Generic;

namespace ChocolArm64.Decoders
{
    class Block
    {
        public ulong Address    { get; set; }
        public ulong EndAddress { get; set; }

        public Block Next   { get; set; }
        public Block Branch { get; set; }

        public List<OpCode64> OpCodes { get; private set; }

        public Block()
        {
            OpCodes = new List<OpCode64>();
        }

        public Block(ulong address) : this()
        {
            Address = address;
        }

        public void Split(Block rightBlock)
        {
            int splitIndex = BinarySearch(OpCodes, rightBlock.Address);

            if ((ulong)OpCodes[splitIndex].Position < rightBlock.Address)
            {
                splitIndex++;
            }

            int splitCount = OpCodes.Count - splitIndex;

            if (splitCount <= 0)
            {
                throw new ArgumentException("Can't split at right block address.");
            }

            rightBlock.EndAddress = EndAddress;

            rightBlock.Next   = Next;
            rightBlock.Branch = Branch;

            rightBlock.OpCodes.AddRange(OpCodes.GetRange(splitIndex, splitCount));

            EndAddress = rightBlock.Address;

            Next   = rightBlock;
            Branch = null;

            OpCodes.RemoveRange(splitIndex, splitCount);
        }

        private static int BinarySearch(List<OpCode64> opCodes, ulong address)
        {
            int left   = 0;
            int middle = 0;
            int right  = opCodes.Count - 1;

            while (left <= right)
            {
                int size = right - left;

                middle = left + (size >> 1);

                OpCode64 opCode = opCodes[middle];

                if (address == (ulong)opCode.Position)
                {
                    break;
                }

                if (address < (ulong)opCode.Position)
                {
                    right = middle - 1;
                }
                else
                {
                    left = middle + 1;
                }
            }

            return middle;
        }

        public OpCode64 GetLastOp()
        {
            if (OpCodes.Count > 0)
            {
                return OpCodes[OpCodes.Count - 1];
            }

            return null;
        }
    }
}
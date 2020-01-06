using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Decoders
{
    class Block
    {
        public ulong Address    { get; set; }
        public ulong EndAddress { get; set; }

        public Block Next   { get; set; }
        public Block Branch { get; set; }

        public OpCodeBranchIndir BrIndir { get; set; }

        public List<OpCode>     OpCodes     { get; }
        public List<OpCodePush> PushOpCodes { get; }

        public Block(ulong address)
        {
            Address = address;

            OpCodes     = new List<OpCode>();
            PushOpCodes = new List<OpCodePush>();
        }

        public void Split(Block rightBlock)
        {
            int splitIndex = BinarySearch(OpCodes, rightBlock.Address);

            if (OpCodes[splitIndex].Address < rightBlock.Address)
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

            rightBlock.UpdatePushOps();

            EndAddress = rightBlock.Address;

            Next   = rightBlock;
            Branch = null;

            OpCodes.RemoveRange(splitIndex, splitCount);

            UpdatePushOps();
        }

        private static int BinarySearch(List<OpCode> opCodes, ulong address)
        {
            int left   = 0;
            int middle = 0;
            int right  = opCodes.Count - 1;

            while (left <= right)
            {
                int size = right - left;

                middle = left + (size >> 1);

                OpCode opCode = opCodes[middle];

                if (address == opCode.Address)
                {
                    break;
                }

                if (address < opCode.Address)
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

        public OpCode GetLastOp()
        {
            if (OpCodes.Count != 0)
            {
                return OpCodes[OpCodes.Count - 1];
            }

            return null;
        }

        public void UpdatePushOps()
        {
            PushOpCodes.Clear();

            for (int index = 0; index < OpCodes.Count; index++)
            {
                if (!(OpCodes[index] is OpCodePush op))
                {
                    continue;
                }

                PushOpCodes.Add(op);
            }
        }
    }
}
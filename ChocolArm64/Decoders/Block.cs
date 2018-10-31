using System.Collections.Generic;

namespace ChocolArm64.Decoders
{
    class Block
    {
        public long Position    { get; set; }
        public long EndPosition { get; set; }

        public Block Next   { get; set; }
        public Block Branch { get; set; }

        public List<OpCode64> OpCodes { get; private set; }

        public Block()
        {
            OpCodes = new List<OpCode64>();
        }

        public Block(long position) : this()
        {
            Position = position;
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
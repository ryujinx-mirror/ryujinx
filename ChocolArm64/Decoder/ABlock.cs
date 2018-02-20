using System.Collections.Generic;

namespace ChocolArm64.Decoder
{
    class ABlock
    {
        public long Position    { get; set; }
        public long EndPosition { get; set; }       

        public ABlock Next   { get; set; }
        public ABlock Branch { get; set; }

        public List<AOpCode> OpCodes { get; private set; }

        public ABlock()
        {
            OpCodes = new List<AOpCode>();
        }

        public ABlock(long Position) : this()
        {
            this.Position = Position;
        }

        public AOpCode GetLastOp()
        {
            if (OpCodes.Count > 0)
            {
                return OpCodes[OpCodes.Count - 1];
            }

            return null;
        }
    }
}
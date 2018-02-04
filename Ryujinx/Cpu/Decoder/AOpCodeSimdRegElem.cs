using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdRegElem : AOpCodeSimd
    {
        public int Rm    { get; private set; }
        public int Index { get; private set; }

        public AOpCodeSimdRegElem(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Rm   = (OpCode >> 16) & 0x1f;
            Size = (OpCode >> 22) & 0x1;

            if (Size != 0)
            {
                Index = (OpCode >> 11) & 1;
            }
            else
            {
                Index = (OpCode >> 21) & 1 |
                        (OpCode >> 10) & 2;
            }
        }
    }
}
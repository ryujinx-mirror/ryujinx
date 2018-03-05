using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdRegElemF : AOpCodeSimdReg
    {
        public int Index { get; private set; }

        public AOpCodeSimdRegElemF(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            if ((Size & 1) != 0)
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
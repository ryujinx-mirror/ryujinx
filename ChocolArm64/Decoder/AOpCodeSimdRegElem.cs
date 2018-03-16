using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdRegElem : AOpCodeSimdReg
    {
        public int Index { get; private set; }

        public AOpCodeSimdRegElem(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            switch (Size)
            {
                case 1:
                    Index = (OpCode >> 20) & 3 |
                            (OpCode >>  9) & 4;

                    Rm &= 0xf;

                    break;

                case 2:
                    Index = (OpCode >> 21) & 1 |
                            (OpCode >> 10) & 2;

                    break;

                default: Emitter = AInstEmit.Und; return;
            }
        }
    }
}
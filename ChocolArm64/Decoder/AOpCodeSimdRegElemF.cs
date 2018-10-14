using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdRegElemF : AOpCodeSimdReg
    {
        public int Index { get; private set; }

        public AOpCodeSimdRegElemF(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            switch ((OpCode >> 21) & 3) // sz:L
            {
                case 0: // H:0
                    Index = (OpCode >> 10) & 2; // 0, 2

                    break;

                case 1: // H:1
                    Index = (OpCode >> 10) & 2;
                    Index++; // 1, 3

                    break;

                case 2: // H
                    Index = (OpCode >> 11) & 1; // 0, 1

                    break;

                default: Emitter = AInstEmit.Und; return;
            }
        }
    }
}

using ChocolArm64.Instruction;
using ChocolArm64.State;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdMemMs : AOpCodeMemReg, IAOpCodeSimd
    {
        public int  Reps   { get; private set; }
        public int  SElems { get; private set; }
        public int  Elems  { get; private set; }
        public bool WBack  { get; private set; }

        public AOpCodeSimdMemMs(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            switch ((OpCode >> 12) & 0xf)
            {
                case 0b0000: Reps = 1; SElems = 4; break;
                case 0b0010: Reps = 4; SElems = 1; break;
                case 0b0100: Reps = 1; SElems = 3; break;
                case 0b0110: Reps = 3; SElems = 1; break;
                case 0b0111: Reps = 1; SElems = 1; break;
                case 0b1000: Reps = 1; SElems = 2; break;
                case 0b1010: Reps = 2; SElems = 1; break;

                default: Inst = AInst.Undefined; return;
            }

            Size  =  (OpCode >> 10) & 0x3;
            WBack = ((OpCode >> 23) & 0x1) != 0;

            bool Q = ((OpCode >> 30) & 1) != 0;

            if (!Q && Size == 3 && SElems != 1)
            {
                Inst = AInst.Undefined;

                return;
            }

            RegisterSize = Q
                ? ARegisterSize.SIMD128
                : ARegisterSize.SIMD64;

            Elems = (GetBitsCount() >> 3) >> Size;
        }
    }
}
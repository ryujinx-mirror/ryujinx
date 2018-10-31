using ChocolArm64.Instructions;
using ChocolArm64.State;

namespace ChocolArm64.Decoders
{
    class OpCodeSimdMemMs64 : OpCodeMemReg64, IOpCodeSimd64
    {
        public int  Reps   { get; private set; }
        public int  SElems { get; private set; }
        public int  Elems  { get; private set; }
        public bool WBack  { get; private set; }

        public OpCodeSimdMemMs64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            switch ((opCode >> 12) & 0xf)
            {
                case 0b0000: Reps = 1; SElems = 4; break;
                case 0b0010: Reps = 4; SElems = 1; break;
                case 0b0100: Reps = 1; SElems = 3; break;
                case 0b0110: Reps = 3; SElems = 1; break;
                case 0b0111: Reps = 1; SElems = 1; break;
                case 0b1000: Reps = 1; SElems = 2; break;
                case 0b1010: Reps = 2; SElems = 1; break;

                default: inst = Inst.Undefined; return;
            }

            Size  =  (opCode >> 10) & 3;
            WBack = ((opCode >> 23) & 1) != 0;

            bool q = ((opCode >> 30) & 1) != 0;

            if (!q && Size == 3 && SElems != 1)
            {
                inst = Inst.Undefined;

                return;
            }

            Extend64 = false;

            RegisterSize = q
                ? State.RegisterSize.Simd128
                : State.RegisterSize.Simd64;

            Elems = (GetBitsCount() >> 3) >> Size;
        }
    }
}
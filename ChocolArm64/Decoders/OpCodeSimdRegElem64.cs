using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeSimdRegElem64 : OpCodeSimdReg64
    {
        public int Index { get; private set; }

        public OpCodeSimdRegElem64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            switch (Size)
            {
                case 1:
                    Index = (opCode >> 20) & 3 |
                            (opCode >>  9) & 4;

                    Rm &= 0xf;

                    break;

                case 2:
                    Index = (opCode >> 21) & 1 |
                            (opCode >> 10) & 2;

                    break;

                default: Emitter = InstEmit.Und; return;
            }
        }
    }
}
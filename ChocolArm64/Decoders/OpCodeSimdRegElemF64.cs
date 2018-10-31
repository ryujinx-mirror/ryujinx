using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeSimdRegElemF64 : OpCodeSimdReg64
    {
        public int Index { get; private set; }

        public OpCodeSimdRegElemF64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            switch ((opCode >> 21) & 3) // sz:L
            {
                case 0: // H:0
                    Index = (opCode >> 10) & 2; // 0, 2

                    break;

                case 1: // H:1
                    Index = (opCode >> 10) & 2;
                    Index++; // 1, 3

                    break;

                case 2: // H
                    Index = (opCode >> 11) & 1; // 0, 1

                    break;

                default: Emitter = InstEmit.Und; return;
            }
        }
    }
}

using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeSimdMemImm64 : OpCodeMemImm64, IOpCodeSimd64
    {
        public OpCodeSimdMemImm64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Size |= (opCode >> 21) & 4;

            if (!WBack && !Unscaled && Size >= 4)
            {
                Imm <<= 4;
            }

            Extend64 = false;
        }
    }
}
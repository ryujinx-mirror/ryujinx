using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeSimdExt64 : OpCodeSimdReg64
    {
        public int Imm4 { get; private set; }

        public OpCodeSimdExt64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Imm4 = (opCode >> 11) & 0xf;
        }
    }
}
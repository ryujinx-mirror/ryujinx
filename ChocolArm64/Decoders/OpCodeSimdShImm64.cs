using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeSimdShImm64 : OpCodeSimd64
    {
        public int Imm { get; private set; }

        public OpCodeSimdShImm64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Imm = (opCode >> 16) & 0x7f;

            Size = BitUtils.HighestBitSetNibble(Imm >> 3);
        }
    }
}

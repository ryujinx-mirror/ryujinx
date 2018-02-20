using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdShImm : AOpCodeSimd
    {
        public int Imm { get; private set; }

        public AOpCodeSimdShImm(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Imm = (OpCode >> 16) & 0x7f;

            Size = ABitUtils.HighestBitSet32(Imm >> 3);
        }
    }
}
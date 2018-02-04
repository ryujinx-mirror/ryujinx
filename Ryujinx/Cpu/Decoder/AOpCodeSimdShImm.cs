using ChocolArm64.Instruction;
using ChocolArm64.State;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdShImm : AOpCode, IAOpCodeSimd
    {
        public int Rd   { get; private set; }
        public int Rn   { get; private set; }
        public int Imm  { get; private set; }
        public int Size { get; private set; }

        public AOpCodeSimdShImm(AInst Inst, long Position, int OpCode) : base(Inst, Position)
        {
            Rd  = (OpCode >>  0) & 0x1f;
            Rn  = (OpCode >>  5) & 0x1f;
            Imm = (OpCode >> 16) & 0x7f;

            Size = ABitUtils.HighestBitSet32(Imm >> 3);

            RegisterSize = ((OpCode >> 30) & 1) != 0
                ? ARegisterSize.SIMD128
                : ARegisterSize.SIMD64;
        }
    }
}
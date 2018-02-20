using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdFmov : AOpCode, IAOpCodeSimd
    {
        public int  Rd   { get; private set; }
        public long Imm  { get; private set; }
        public int  Size { get; private set; }

        public AOpCodeSimdFmov(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            int Imm5 = (OpCode >>  5) & 0x1f;
            int Type = (OpCode >> 22) & 0x3;

            if (Imm5 != 0b00000 || Type > 1)
            {
                Emitter = AInstEmit.Und;

                return;
            }

            Size = Type;

            long Imm;

            Rd  = (OpCode >>  0) & 0x1f;
            Imm = (OpCode >> 13) & 0xff;

            this.Imm = ADecoderHelper.DecodeImm8Float(Imm, Type);
        }
    }
}
using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeSimdFmov64 : OpCode64, IOpCodeSimd64
    {
        public int  Rd   { get; private set; }
        public long Imm  { get; private set; }
        public int  Size { get; private set; }

        public OpCodeSimdFmov64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            int imm5 = (opCode >>  5) & 0x1f;
            int type = (opCode >> 22) & 0x3;

            if (imm5 != 0b00000 || type > 1)
            {
                Emitter = InstEmit.Und;

                return;
            }

            Size = type;

            long imm;

            Rd  = (opCode >>  0) & 0x1f;
            imm = (opCode >> 13) & 0xff;

            Imm = DecoderHelper.DecodeImm8Float(imm, type);
        }
    }
}
namespace ARMeilleure.Decoders
{
    class OpCodeSimdFmov : OpCode, IOpCodeSimd
    {
        public int  Rd        { get; private set; }
        public long Immediate { get; private set; }
        public int  Size      { get; private set; }

        public OpCodeSimdFmov(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            int imm5 = (opCode >>  5) & 0x1f;
            int type = (opCode >> 22) & 0x3;

            if (imm5 != 0b00000 || type > 1)
            {
                Instruction = InstDescriptor.Undefined;

                return;
            }

            Size = type;

            long imm;

            Rd  = (opCode >>  0) & 0x1f;
            imm = (opCode >> 13) & 0xff;

            Immediate = DecoderHelper.DecodeImm8Float(imm, type);
        }
    }
}
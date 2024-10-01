namespace ARMeilleure.Decoders
{
    class OpCodeSimdFmov : OpCode, IOpCodeSimd
    {
        public int Rd { get; }
        public long Immediate { get; }
        public int Size { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeSimdFmov(inst, address, opCode);

        public OpCodeSimdFmov(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            int type = (opCode >> 22) & 0x3;

            Size = type;

            long imm;

            Rd = (opCode >> 0) & 0x1f;
            imm = (opCode >> 13) & 0xff;

            if (type == 0)
            {
                Immediate = (long)DecoderHelper.Imm8ToFP32Table[(int)imm];
            }
            else /* if (type == 1) */
            {
                Immediate = (long)DecoderHelper.Imm8ToFP64Table[(int)imm];
            }
        }
    }
}

namespace ARMeilleure.Decoders
{
    class OpCodeMemPair : OpCodeMemImm
    {
        public int Rt2 { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeMemPair(inst, address, opCode);

        public OpCodeMemPair(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rt2 = (opCode >> 10) & 0x1f;
            WBack = ((opCode >> 23) & 0x1) != 0;
            PostIdx = ((opCode >> 23) & 0x3) == 1;
            Extend64 = ((opCode >> 30) & 0x3) == 1;
            Size = ((opCode >> 31) & 0x1) | 2;

            DecodeImm(opCode);
        }

        protected void DecodeImm(int opCode)
        {
            Immediate = ((long)(opCode >> 15) << 57) >> (57 - Size);
        }
    }
}

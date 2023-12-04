namespace ARMeilleure.Decoders
{
    class OpCodeT16BImm8 : OpCodeT16, IOpCode32BImm
    {
        public long Immediate { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT16BImm8(inst, address, opCode);

        public OpCodeT16BImm8(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Cond = (Condition)((opCode >> 8) & 0xf);

            int imm = (opCode << 24) >> 23;
            Immediate = GetPc() + imm;
        }
    }
}

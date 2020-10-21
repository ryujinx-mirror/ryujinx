namespace ARMeilleure.Decoders
{
    class OpCode32AluImm16 : OpCode32Alu
    {
        public int Immediate { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32AluImm16(inst, address, opCode);

        public OpCode32AluImm16(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            int imm12 = opCode & 0xfff;
            int imm4 = (opCode >> 16) & 0xf;

            Immediate = (imm4 << 12) | imm12;
        }
    }
}

namespace ARMeilleure.Decoders
{
    class OpCodeT16BImm11 : OpCodeT16, IOpCode32BImm
    {
        public long Immediate { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT16BImm11(inst, address, opCode);

        public OpCodeT16BImm11(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            int imm = (opCode << 21) >> 20; 
            Immediate = GetPc() + imm;
        }
    }
}

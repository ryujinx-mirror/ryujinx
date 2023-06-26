namespace ARMeilleure.Decoders
{
    class OpCode32MemImm8 : OpCode32Mem
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32MemImm8(inst, address, opCode);

        public OpCode32MemImm8(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            int imm4L = (opCode >> 0) & 0xf;
            int imm4H = (opCode >> 8) & 0xf;

            Immediate = imm4L | (imm4H << 4);
        }
    }
}

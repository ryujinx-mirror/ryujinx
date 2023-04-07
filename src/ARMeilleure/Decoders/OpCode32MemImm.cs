namespace ARMeilleure.Decoders
{
    class OpCode32MemImm : OpCode32Mem
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32MemImm(inst, address, opCode);

        public OpCode32MemImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Immediate = opCode & 0xfff;
        }
    }
}
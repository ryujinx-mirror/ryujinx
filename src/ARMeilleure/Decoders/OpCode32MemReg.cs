namespace ARMeilleure.Decoders
{
    class OpCode32MemReg : OpCode32Mem, IOpCode32MemReg
    {
        public int Rm { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32MemReg(inst, address, opCode);

        public OpCode32MemReg(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm = (opCode >> 0) & 0xf;
        }
    }
}

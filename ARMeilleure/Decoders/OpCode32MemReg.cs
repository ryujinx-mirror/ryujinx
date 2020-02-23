namespace ARMeilleure.Decoders
{
    class OpCode32MemReg : OpCode32Mem
    {
        public int Rm { get; private set; }

        public OpCode32MemReg(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm = (opCode >> 0) & 0xf;
        }
    }
}

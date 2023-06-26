namespace ARMeilleure.Decoders
{
    class OpCodeT32Tb : OpCodeT32, IOpCode32BReg
    {
        public int Rm { get; }
        public int Rn { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT32Tb(inst, address, opCode);

        public OpCodeT32Tb(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm = (opCode >> 0) & 0xf;
            Rn = (opCode >> 16) & 0xf;
        }
    }
}

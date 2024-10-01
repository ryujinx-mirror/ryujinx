namespace ARMeilleure.Decoders
{
    class OpCodeT32AluReg : OpCodeT32Alu, IOpCode32AluReg
    {
        public int Rm { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT32AluReg(inst, address, opCode);

        public OpCodeT32AluReg(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm = (opCode >> 0) & 0xf;
        }
    }
}

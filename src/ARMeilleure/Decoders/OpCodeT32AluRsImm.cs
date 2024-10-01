namespace ARMeilleure.Decoders
{
    class OpCodeT32AluRsImm : OpCodeT32Alu, IOpCode32AluRsImm
    {
        public int Rm { get; }
        public int Immediate { get; }

        public ShiftType ShiftType { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT32AluRsImm(inst, address, opCode);

        public OpCodeT32AluRsImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm = (opCode >> 0) & 0xf;
            Immediate = ((opCode >> 6) & 3) | ((opCode >> 10) & 0x1c);

            ShiftType = (ShiftType)((opCode >> 4) & 3);
        }
    }
}

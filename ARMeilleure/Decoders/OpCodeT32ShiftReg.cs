namespace ARMeilleure.Decoders
{
    class OpCodeT32ShiftReg : OpCodeT32Alu, IOpCode32AluRsReg
    {
        public int Rm => Rn;
        public int Rs { get; }

        public ShiftType ShiftType { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT32ShiftReg(inst, address, opCode);

        public OpCodeT32ShiftReg(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rs = (opCode >> 0) & 0xf;

            ShiftType = (ShiftType)((opCode >> 21) & 3);
        }
    }
}

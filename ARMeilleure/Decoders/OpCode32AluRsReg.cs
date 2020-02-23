namespace ARMeilleure.Decoders
{
    class OpCode32AluRsReg : OpCode32Alu
    {
        public int Rm { get; private set; }
        public int Rs { get; private set; }

        public ShiftType ShiftType { get; private set; }

        public OpCode32AluRsReg(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm = (opCode >> 0) & 0xf;
            Rs = (opCode >> 8) & 0xf;

            ShiftType = (ShiftType)((opCode >> 5) & 3);
        }
    }
}

namespace ARMeilleure.Decoders
{
    class OpCode32AluRsImm : OpCode32Alu
    {
        public int Rm  { get; private set; }
        public int Imm { get; private set; }

        public ShiftType ShiftType { get; private set; }

        public OpCode32AluRsImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm  = (opCode >> 0) & 0xf;
            Imm = (opCode >> 7) & 0x1f;

            ShiftType = (ShiftType)((opCode >> 5) & 3);
        }
    }
}
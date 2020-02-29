namespace ARMeilleure.Decoders
{
    class OpCode32Sat : OpCode32
    {
        public int Rn { get; private set; }
        public int Imm5 { get; private set; }
        public int Rd { get; private set; }
        public int SatImm { get; private set; }

        public ShiftType ShiftType { get; private set; }

        public OpCode32Sat(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rn = (opCode >> 0) & 0xf;
            Imm5 = (opCode >> 7) & 0x1f;
            Rd = (opCode >> 12) & 0xf;
            SatImm = (opCode >> 16) & 0x1f;

            ShiftType = (ShiftType)((opCode >> 5) & 2);
        }
    }
}
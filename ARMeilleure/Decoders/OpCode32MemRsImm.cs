namespace ARMeilleure.Decoders
{
    class OpCode32MemRsImm : OpCode32Mem
    {
        public int Rm { get; private set; }
        public ShiftType ShiftType { get; private set; }

        public OpCode32MemRsImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm = (opCode >> 0) & 0xf;
            Immediate = (opCode >> 7) & 0x1f;

            ShiftType = (ShiftType)((opCode >> 5) & 3);
        }
    }
}

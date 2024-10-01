namespace ARMeilleure.Decoders
{
    class OpCode32MemRsImm : OpCode32Mem, IOpCode32MemRsImm
    {
        public int Rm { get; }
        public ShiftType ShiftType { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32MemRsImm(inst, address, opCode);

        public OpCode32MemRsImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm = (opCode >> 0) & 0xf;
            Immediate = (opCode >> 7) & 0x1f;

            ShiftType = (ShiftType)((opCode >> 5) & 3);
        }
    }
}

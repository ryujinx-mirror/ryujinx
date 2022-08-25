namespace ARMeilleure.Decoders
{
    class OpCodeT32MemRsImm : OpCodeT32, IOpCode32MemRsImm
    {
        public int Rt { get; }
        public int Rn { get; }
        public int Rm { get; }
        public ShiftType ShiftType => ShiftType.Lsl;

        public bool WBack => false;
        public bool IsLoad { get; }
        public bool Index => true;
        public bool Add => true;

        public int Immediate { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT32MemRsImm(inst, address, opCode);

        public OpCodeT32MemRsImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm = (opCode >> 0) & 0xf;
            Rt = (opCode >> 12) & 0xf;
            Rn = (opCode >> 16) & 0xf;

            IsLoad = (opCode & (1 << 20)) != 0;

            Immediate = (opCode >> 4) & 3;
        }
    }
}

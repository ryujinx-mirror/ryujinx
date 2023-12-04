namespace ARMeilleure.Decoders
{
    class OpCodeT16MemReg : OpCodeT16, IOpCode32MemReg
    {
        public int Rm { get; }
        public int Rt { get; }
        public int Rn { get; }

        public bool WBack => false;
        public bool IsLoad { get; }
        public bool Index => true;
        public bool Add => true;

        public int Immediate => throw new System.InvalidOperationException();

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT16MemReg(inst, address, opCode);

        public OpCodeT16MemReg(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rt = (opCode >> 0) & 7;
            Rn = (opCode >> 3) & 7;
            Rm = (opCode >> 6) & 7;

            IsLoad = ((opCode >> 9) & 7) >= 3;
        }
    }
}

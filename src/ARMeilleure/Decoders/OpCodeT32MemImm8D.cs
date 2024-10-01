namespace ARMeilleure.Decoders
{
    class OpCodeT32MemImm8D : OpCodeT32, IOpCode32Mem
    {
        public int Rt { get; }
        public int Rt2 { get; }
        public int Rn { get; }
        public bool WBack { get; }
        public bool IsLoad { get; }
        public bool Index { get; }
        public bool Add { get; }
        public int Immediate { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT32MemImm8D(inst, address, opCode);

        public OpCodeT32MemImm8D(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rt2 = (opCode >> 8) & 0xf;
            Rt = (opCode >> 12) & 0xf;
            Rn = (opCode >> 16) & 0xf;

            Index = ((opCode >> 24) & 1) != 0;
            Add = ((opCode >> 23) & 1) != 0;
            WBack = ((opCode >> 21) & 1) != 0;

            Immediate = (opCode & 0xff) << 2;

            IsLoad = ((opCode >> 20) & 1) != 0;
        }
    }
}

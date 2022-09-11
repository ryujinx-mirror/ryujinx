namespace ARMeilleure.Decoders
{
    class OpCodeT32MemStEx : OpCodeT32, IOpCode32MemEx
    {
        public int Rd { get; }
        public int Rt { get; }
        public int Rt2 { get; }
        public int Rn { get; }

        public bool WBack => false;
        public bool IsLoad => false;
        public bool Index => false;
        public bool Add => false;

        public int Immediate => 0;

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT32MemStEx(inst, address, opCode);

        public OpCodeT32MemStEx(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd = (opCode >> 0) & 0xf;
            Rt2 = (opCode >> 8) & 0xf;
            Rt = (opCode >> 12) & 0xf;
            Rn = (opCode >> 16) & 0xf;
        }
    }
}

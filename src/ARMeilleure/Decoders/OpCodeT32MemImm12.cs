namespace ARMeilleure.Decoders
{
    class OpCodeT32MemImm12 : OpCodeT32, IOpCode32Mem
    {
        public int Rt { get; }
        public int Rn { get; }
        public bool WBack => false;
        public bool IsLoad { get; }
        public bool Index => true;
        public bool Add => true;
        public int Immediate { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT32MemImm12(inst, address, opCode);

        public OpCodeT32MemImm12(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rt = (opCode >> 12) & 0xf;
            Rn = (opCode >> 16) & 0xf;

            Immediate = opCode & 0xfff;

            IsLoad = ((opCode >> 20) & 1) != 0;
        }
    }
}
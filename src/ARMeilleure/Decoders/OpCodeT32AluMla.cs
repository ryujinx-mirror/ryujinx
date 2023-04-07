namespace ARMeilleure.Decoders
{
    class OpCodeT32AluMla : OpCodeT32, IOpCode32AluMla
    {
        public int Rn { get; }
        public int Rm { get; }
        public int Ra { get; }
        public int Rd { get; }

        public bool NHigh { get; }
        public bool MHigh { get; }
        public bool R { get; }
        public bool? SetFlags => false;

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT32AluMla(inst, address, opCode);

        public OpCodeT32AluMla(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm = (opCode >> 0) & 0xf;
            Rd = (opCode >> 8) & 0xf;
            Ra = (opCode >> 12) & 0xf;
            Rn = (opCode >> 16) & 0xf;
            R = (opCode & (1 << 4)) != 0;

            MHigh = ((opCode >> 4) & 0x1) == 1;
            NHigh = ((opCode >> 5) & 0x1) == 1;
        }
    }
}

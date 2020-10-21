namespace ARMeilleure.Decoders
{
    class OpCode32AluMla : OpCode32, IOpCode32AluReg
    {
        public int Rn { get; }
        public int Rm { get; }
        public int Ra { get; }
        public int Rd { get; }

        public bool NHigh { get; }
        public bool MHigh { get; }
        public bool R { get; }
        public bool SetFlags { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32AluMla(inst, address, opCode);

        public OpCode32AluMla(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rn = (opCode >> 0) & 0xf;
            Rm = (opCode >> 8) & 0xf;
            Ra = (opCode >> 12) & 0xf;
            Rd = (opCode >> 16) & 0xf;
            R = (opCode & (1 << 5)) != 0;

            NHigh = ((opCode >> 5) & 0x1) == 1;
            MHigh = ((opCode >> 6) & 0x1) == 1;
            SetFlags = ((opCode >> 20) & 1) != 0;
        }
    }
}

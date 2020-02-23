namespace ARMeilleure.Decoders
{
    class OpCode32AluMla : OpCode32, IOpCode32AluReg
    {
        public int Rn { get; private set; }
        public int Rm { get; private set; }
        public int Ra { get; private set; }
        public int Rd { get; private set; }

        public bool NHigh { get; private set; }
        public bool MHigh { get; private set; }
        public bool R { get; private set; }
        public bool SetFlags { get; private set; }

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

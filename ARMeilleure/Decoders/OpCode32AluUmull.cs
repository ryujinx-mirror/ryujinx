namespace ARMeilleure.Decoders
{
    class OpCode32AluUmull : OpCode32
    {
        public int RdLo { get; private set; }
        public int RdHi { get; private set; }
        public int Rn { get; private set; }
        public int Rm { get; private set; }

        public bool NHigh { get; private set; }
        public bool MHigh { get; private set; }

        public bool SetFlags { get; private set; }
        public DataOp DataOp { get; private set; }

        public OpCode32AluUmull(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            RdLo = (opCode >> 12) & 0xf;
            RdHi = (opCode >> 16) & 0xf;
            Rm = (opCode >> 8) & 0xf;
            Rn = (opCode >> 0) & 0xf;

            NHigh = ((opCode >> 5) & 0x1) == 1;
            MHigh = ((opCode >> 6) & 0x1) == 1;

            SetFlags = ((opCode >> 20) & 0x1) != 0;
            DataOp = DataOp.Arithmetic;
        }
    }
}

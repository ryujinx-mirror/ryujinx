namespace ARMeilleure.Decoders
{
    class OpCode32AluUmull : OpCode32
    {
        public int RdLo { get; }
        public int RdHi { get; }
        public int Rn { get; }
        public int Rm { get; }

        public bool NHigh { get; }
        public bool MHigh { get; }

        public bool SetFlags { get; }
        public DataOp DataOp { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32AluUmull(inst, address, opCode);

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

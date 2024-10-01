namespace ARMeilleure.Decoders
{
    class OpCodeT32AluUmull : OpCodeT32, IOpCode32AluUmull
    {
        public int RdLo { get; }
        public int RdHi { get; }
        public int Rn { get; }
        public int Rm { get; }

        public bool NHigh { get; }
        public bool MHigh { get; }

        public bool? SetFlags => false;

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT32AluUmull(inst, address, opCode);

        public OpCodeT32AluUmull(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm = (opCode >> 0) & 0xf;
            RdHi = (opCode >> 8) & 0xf;
            RdLo = (opCode >> 12) & 0xf;
            Rn = (opCode >> 16) & 0xf;

            MHigh = ((opCode >> 4) & 0x1) == 1;
            NHigh = ((opCode >> 5) & 0x1) == 1;
        }
    }
}

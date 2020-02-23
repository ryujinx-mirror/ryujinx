namespace ARMeilleure.Decoders
{
    class OpCode32AluBf : OpCode32, IOpCode32AluBf
    {
        public int Rd { get; private set; }
        public int Rn { get; private set; }

        public int Msb { get; private set; }

        public int Lsb { get; private set; }

        public int SourceMask => (int)(0xFFFFFFFF >> (31 - Msb));
        public int DestMask => SourceMask & (int)(0xFFFFFFFF << Lsb);

        public OpCode32AluBf(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd = (opCode >> 12) & 0xf;
            Rn = (opCode >> 0) & 0xf;

            Msb = (opCode >> 16) & 0x1f;
            Lsb = (opCode >> 7) & 0x1f;
        }
    }
}

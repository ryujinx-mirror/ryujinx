namespace ARMeilleure.Decoders
{
    class OpCodeT32AluBf : OpCodeT32, IOpCode32AluBf
    {
        public int Rd { get; }
        public int Rn { get; }

        public int Msb { get; }
        public int Lsb { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT32AluBf(inst, address, opCode);

        public OpCodeT32AluBf(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd = (opCode >> 8) & 0xf;
            Rn = (opCode >> 16) & 0xf;

            Msb = (opCode >> 0) & 0x1f;
            Lsb = ((opCode >> 6) & 0x3) | ((opCode >> 10) & 0x1c);
        }
    }
}

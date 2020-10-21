namespace ARMeilleure.Decoders
{
    class OpCode32Sat16 : OpCode32
    {
        public int Rn { get; }
        public int Rd { get; }
        public int SatImm { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32Sat16(inst, address, opCode);

        public OpCode32Sat16(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rn = (opCode >> 0) & 0xf;
            Rd = (opCode >> 12) & 0xf;
            SatImm = (opCode >> 16) & 0xf;
        }
    }
}
namespace ARMeilleure.Decoders
{
    class OpCode32Sat16 : OpCode32
    {
        public int Rn { get; private set; }
        public int Rd { get; private set; }
        public int SatImm { get; private set; }

        public OpCode32Sat16(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rn = (opCode >> 0) & 0xf;
            Rd = (opCode >> 12) & 0xf;
            SatImm = (opCode >> 16) & 0xf;
        }
    }
}
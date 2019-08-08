namespace ARMeilleure.Decoders
{
    class OpCode32Alu : OpCode32, IOpCode32Alu
    {
        public int Rd { get; private set; }
        public int Rn { get; private set; }

        public bool SetFlags { get; private set; }

        public OpCode32Alu(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd = (opCode >> 12) & 0xf;
            Rn = (opCode >> 16) & 0xf;

            SetFlags = ((opCode >> 20) & 1) != 0;
        }
    }
}
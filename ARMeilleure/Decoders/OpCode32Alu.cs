namespace ARMeilleure.Decoders
{
    class OpCode32Alu : OpCode32, IOpCode32Alu
    {
        public int Rd { get; }
        public int Rn { get; }

        public bool SetFlags { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32Alu(inst, address, opCode);

        public OpCode32Alu(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd = (opCode >> 12) & 0xf;
            Rn = (opCode >> 16) & 0xf;

            SetFlags = ((opCode >> 20) & 1) != 0;
        }
    }
}
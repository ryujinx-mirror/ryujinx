namespace ARMeilleure.Decoders
{
    class OpCode32Mrs : OpCode32
    {
        public bool R { get; }
        public int Rd { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32Mrs(inst, address, opCode);

        public OpCode32Mrs(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            R = ((opCode >> 22) & 1) != 0;
            Rd = (opCode >> 12) & 0xf;
        }
    }
}

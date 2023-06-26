namespace ARMeilleure.Decoders
{
    class OpCodeCsel : OpCodeAlu, IOpCodeCond
    {
        public int Rm { get; }

        public Condition Cond { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeCsel(inst, address, opCode);

        public OpCodeCsel(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm = (opCode >> 16) & 0x1f;
            Cond = (Condition)((opCode >> 12) & 0xf);
        }
    }
}

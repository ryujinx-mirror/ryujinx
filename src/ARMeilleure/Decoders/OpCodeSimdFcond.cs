namespace ARMeilleure.Decoders
{
    class OpCodeSimdFcond : OpCodeSimdReg, IOpCodeCond
    {
        public int Nzcv { get; }

        public Condition Cond { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeSimdFcond(inst, address, opCode);

        public OpCodeSimdFcond(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Nzcv = (opCode >> 0) & 0xf;
            Cond = (Condition)((opCode >> 12) & 0xf);
        }
    }
}

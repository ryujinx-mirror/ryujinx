namespace ARMeilleure.Decoders
{
    class OpCodeSimdFcond : OpCodeSimdReg, IOpCodeCond
    {
        public int Nzcv { get; private set; }

        public Condition Cond { get; private set; }

        public OpCodeSimdFcond(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Nzcv =             (opCode >>  0) & 0xf;
            Cond = (Condition)((opCode >> 12) & 0xf);
        }
    }
}

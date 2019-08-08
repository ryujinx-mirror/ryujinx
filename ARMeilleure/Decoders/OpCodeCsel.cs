namespace ARMeilleure.Decoders
{
    class OpCodeCsel : OpCodeAlu, IOpCodeCond
    {
        public int Rm { get; private set; }

        public Condition Cond { get; private set; }

        public OpCodeCsel(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm   =             (opCode >> 16) & 0x1f;
            Cond = (Condition)((opCode >> 12) & 0xf);
        }
    }
}
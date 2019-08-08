namespace ARMeilleure.Decoders
{
    class OpCodeMul : OpCodeAlu
    {
        public int Rm { get; private set; }
        public int Ra { get; private set; }

        public OpCodeMul(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Ra = (opCode >> 10) & 0x1f;
            Rm = (opCode >> 16) & 0x1f;
        }
    }
}
namespace ARMeilleure.Decoders
{
    class OpCodeMemReg : OpCodeMem
    {
        public bool Shift { get; private set; }
        public int  Rm    { get; private set; }

        public IntType IntType { get; private set; }

        public OpCodeMemReg(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Shift    =          ((opCode >> 12) & 0x1) != 0;
            IntType  = (IntType)((opCode >> 13) & 0x7);
            Rm       =           (opCode >> 16) & 0x1f;
            Extend64 =          ((opCode >> 22) & 0x3) == 2;
        }
    }
}
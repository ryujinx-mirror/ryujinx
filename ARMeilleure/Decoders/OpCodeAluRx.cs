namespace ARMeilleure.Decoders
{
    class OpCodeAluRx : OpCodeAlu, IOpCodeAluRx
    {
        public int Shift { get; private set; }
        public int Rm    { get; private set; }

        public IntType IntType { get; private set; }

        public OpCodeAluRx(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Shift   =           (opCode >> 10) & 0x7;
            IntType = (IntType)((opCode >> 13) & 0x7);
            Rm      =           (opCode >> 16) & 0x1f;
        }
    }
}
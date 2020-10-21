namespace ARMeilleure.Decoders
{
    class OpCodeAluRx : OpCodeAlu, IOpCodeAluRx
    {
        public int Shift { get; }
        public int Rm    { get; }

        public IntType IntType { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeAluRx(inst, address, opCode);

        public OpCodeAluRx(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Shift   =           (opCode >> 10) & 0x7;
            IntType = (IntType)((opCode >> 13) & 0x7);
            Rm      =           (opCode >> 16) & 0x1f;
        }
    }
}
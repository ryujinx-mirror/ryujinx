namespace ARMeilleure.Decoders
{
    class OpCodeT16AluRegLow : OpCodeT16, IOpCode32AluReg
    {
        public int Rm { get; }
        public int Rd { get; }
        public int Rn { get; }

        public bool? SetFlags => null;

        public static new OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT16AluRegLow(inst, address, opCode);

        public OpCodeT16AluRegLow(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd = (opCode >> 0) & 0x7;
            Rn = (opCode >> 0) & 0x7;
            Rm = (opCode >> 3) & 0x7;
        }
    }
}

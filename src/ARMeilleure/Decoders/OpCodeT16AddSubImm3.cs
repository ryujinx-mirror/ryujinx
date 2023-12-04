namespace ARMeilleure.Decoders
{
    class OpCodeT16AddSubImm3 : OpCodeT16, IOpCode32AluImm
    {
        public int Rd { get; }
        public int Rn { get; }

        public bool? SetFlags => null;

        public int Immediate { get; }

        public bool IsRotated { get; }

        public static new OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT16AddSubImm3(inst, address, opCode);

        public OpCodeT16AddSubImm3(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd = (opCode >> 0) & 0x7;
            Rn = (opCode >> 3) & 0x7;
            Immediate = (opCode >> 6) & 0x7;
            IsRotated = false;
        }
    }
}

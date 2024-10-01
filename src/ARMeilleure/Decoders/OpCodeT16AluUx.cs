namespace ARMeilleure.Decoders
{
    class OpCodeT16AluUx : OpCodeT16, IOpCode32AluUx
    {
        public int Rm { get; }
        public int Rd { get; }
        public int Rn { get; }

        public bool? SetFlags => false;

        public int RotateBits => 0;
        public bool Add => false;

        public static new OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT16AluUx(inst, address, opCode);

        public OpCodeT16AluUx(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd = (opCode >> 0) & 0x7;
            Rm = (opCode >> 3) & 0x7;
        }
    }
}

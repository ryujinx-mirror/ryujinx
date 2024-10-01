namespace ARMeilleure.Decoders
{
    class OpCodeSimd : OpCode, IOpCodeSimd
    {
        public int Rd { get; }
        public int Rn { get; }
        public int Opc { get; }
        public int Size { get; protected set; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeSimd(inst, address, opCode);

        public OpCodeSimd(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd = (opCode >> 0) & 0x1f;
            Rn = (opCode >> 5) & 0x1f;
            Opc = (opCode >> 15) & 0x3;
            Size = (opCode >> 22) & 0x3;

            RegisterSize = ((opCode >> 30) & 1) != 0
                ? RegisterSize.Simd128
                : RegisterSize.Simd64;
        }
    }
}

namespace ARMeilleure.Decoders
{
    class OpCode32SimdRegLong : OpCode32SimdReg
    {
        public bool Polynomial { get; private set; }

        public OpCode32SimdRegLong(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Q = false;
            RegisterSize = RegisterSize.Simd64;
            Polynomial = ((opCode >> 9) & 0x1) != 0;
        }
    }
}

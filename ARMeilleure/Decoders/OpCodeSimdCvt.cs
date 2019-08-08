namespace ARMeilleure.Decoders
{
    class OpCodeSimdCvt : OpCodeSimd
    {
        public int FBits { get; private set; }

        public OpCodeSimdCvt(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            int scale = (opCode >> 10) & 0x3f;
            int sf    = (opCode >> 31) & 0x1;

            FBits = 64 - scale;

            RegisterSize = sf != 0
                ? RegisterSize.Int64
                : RegisterSize.Int32;
        }
    }
}
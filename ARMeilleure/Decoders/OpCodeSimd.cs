namespace ARMeilleure.Decoders
{
    class OpCodeSimd : OpCode, IOpCodeSimd
    {
        public int Rd   { get; private   set; }
        public int Rn   { get; private   set; }
        public int Opc  { get; private   set; }
        public int Size { get; protected set; }

        public OpCodeSimd(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd   = (opCode >>  0) & 0x1f;
            Rn   = (opCode >>  5) & 0x1f;
            Opc  = (opCode >> 15) & 0x3;
            Size = (opCode >> 22) & 0x3;

            RegisterSize = ((opCode >> 30) & 1) != 0
                ? RegisterSize.Simd128
                : RegisterSize.Simd64;
        }
    }
}
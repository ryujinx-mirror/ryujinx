namespace ARMeilleure.Decoders
{
    class OpCodeSimdReg : OpCodeSimd
    {
        public bool Bit3 { get; private   set; }
        public int  Ra   { get; private   set; }
        public int  Rm   { get; protected set; }

        public OpCodeSimdReg(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Bit3 = ((opCode >>  3) & 0x1) != 0;
            Ra   =  (opCode >> 10) & 0x1f;
            Rm   =  (opCode >> 16) & 0x1f;
        }
    }
}
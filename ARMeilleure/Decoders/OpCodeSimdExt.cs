namespace ARMeilleure.Decoders
{
    class OpCodeSimdExt : OpCodeSimdReg
    {
        public int Imm4 { get; private set; }

        public OpCodeSimdExt(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Imm4 = (opCode >> 11) & 0xf;
        }
    }
}
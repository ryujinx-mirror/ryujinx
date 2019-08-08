namespace ARMeilleure.Decoders
{
    class OpCodeSimdMemReg : OpCodeMemReg, IOpCodeSimd
    {
        public OpCodeSimdMemReg(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Size |= (opCode >> 21) & 4;

            Extend64 = false;
        }
    }
}
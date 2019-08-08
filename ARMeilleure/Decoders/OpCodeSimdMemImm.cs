namespace ARMeilleure.Decoders
{
    class OpCodeSimdMemImm : OpCodeMemImm, IOpCodeSimd
    {
        public OpCodeSimdMemImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Size |= (opCode >> 21) & 4;

            if (!WBack && !Unscaled && Size >= 4)
            {
                Immediate <<= 4;
            }

            Extend64 = false;
        }
    }
}
namespace ARMeilleure.Decoders
{
    class OpCodeSimdMemImm : OpCodeMemImm, IOpCodeSimd
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeSimdMemImm(inst, address, opCode);

        public OpCodeSimdMemImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Size |= (opCode >> 21) & 4;

            if (Size > 4)
            {
                Instruction = InstDescriptor.Undefined;

                return;
            }

            // Base class already shifts the immediate, we only
            // need to shift it if size (scale) is 4, since this value is only set here.
            if (!WBack && !Unscaled && Size == 4)
            {
                Immediate <<= 4;
            }

            Extend64 = false;
        }
    }
}

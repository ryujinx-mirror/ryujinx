namespace ARMeilleure.Decoders
{
    class OpCodeSimdMemReg : OpCodeMemReg, IOpCodeSimd
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeSimdMemReg(inst, address, opCode);

        public OpCodeSimdMemReg(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Size |= (opCode >> 21) & 4;

            if (Size > 4)
            {
                Instruction = InstDescriptor.Undefined;

                return;
            }

            Extend64 = false;
        }
    }
}

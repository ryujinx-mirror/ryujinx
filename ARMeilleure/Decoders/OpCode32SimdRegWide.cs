namespace ARMeilleure.Decoders
{
    class OpCode32SimdRegWide : OpCode32SimdReg
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdRegWide(inst, address, opCode);

        public OpCode32SimdRegWide(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Q = false;
            RegisterSize = RegisterSize.Simd64;

            // Subclasses have their own handling of Vx to account for before checking.
            if (GetType() == typeof(OpCode32SimdRegWide) && DecoderHelper.VectorArgumentsInvalid(true, Vd, Vn))
            {
                Instruction = InstDescriptor.Undefined;
            }
        }
    }
}

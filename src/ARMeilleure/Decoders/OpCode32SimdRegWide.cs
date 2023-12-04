namespace ARMeilleure.Decoders
{
    class OpCode32SimdRegWide : OpCode32SimdReg
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdRegWide(inst, address, opCode, false);
        public new static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdRegWide(inst, address, opCode, true);

        public OpCode32SimdRegWide(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode, isThumb)
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

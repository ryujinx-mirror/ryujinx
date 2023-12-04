namespace ARMeilleure.Decoders
{
    class OpCode32SimdSqrte : OpCode32Simd
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdSqrte(inst, address, opCode, false);
        public new static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdSqrte(inst, address, opCode, true);

        public OpCode32SimdSqrte(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode, isThumb)
        {
            Size = (opCode >> 18) & 0x1;
            F = ((opCode >> 8) & 0x1) != 0;

            if (DecoderHelper.VectorArgumentsInvalid(Q, Vd, Vm))
            {
                Instruction = InstDescriptor.Undefined;
            }
        }
    }
}

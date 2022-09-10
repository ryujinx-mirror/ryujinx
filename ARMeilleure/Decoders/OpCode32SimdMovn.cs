namespace ARMeilleure.Decoders
{
    class OpCode32SimdMovn : OpCode32Simd
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdMovn(inst, address, opCode);

        public OpCode32SimdMovn(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Size = (opCode >> 18) & 0x3;
        }
    }
}

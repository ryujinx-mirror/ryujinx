namespace ARMeilleure.Decoders
{
    class OpCode32SimdMovn : OpCode32Simd
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdMovn(inst, address, opCode, false);
        public new static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdMovn(inst, address, opCode, true);

        public OpCode32SimdMovn(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode, isThumb)
        {
            Size = (opCode >> 18) & 0x3;
        }
    }
}

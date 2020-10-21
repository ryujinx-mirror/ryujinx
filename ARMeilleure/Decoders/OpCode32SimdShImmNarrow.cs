namespace ARMeilleure.Decoders
{
    class OpCode32SimdShImmNarrow : OpCode32SimdShImm
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdShImmNarrow(inst, address, opCode);

        public OpCode32SimdShImmNarrow(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode) { }
    }
}

namespace ARMeilleure.Decoders
{
    class OpCode32SimdSel : OpCode32SimdRegS
    {
        public OpCode32SimdSelMode Cc { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdSel(inst, address, opCode, false);
        public new static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdSel(inst, address, opCode, true);

        public OpCode32SimdSel(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode, isThumb)
        {
            Cc = (OpCode32SimdSelMode)((opCode >> 20) & 3);
        }
    }

    enum OpCode32SimdSelMode
    {
        Eq = 0,
        Vs,
        Ge,
        Gt,
    }
}

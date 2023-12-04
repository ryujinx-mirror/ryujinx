namespace ARMeilleure.Decoders
{
    class OpCode32SimdSpecial : OpCode32
    {
        public int Rt { get; }
        public int Sreg { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdSpecial(inst, address, opCode, false);
        public static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdSpecial(inst, address, opCode, true);

        public OpCode32SimdSpecial(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode)
        {
            IsThumb = isThumb;

            Rt = (opCode >> 12) & 0xf;
            Sreg = (opCode >> 16) & 0xf;
        }
    }
}

namespace ARMeilleure.Decoders
{
    class OpCode32SimdRegS : OpCode32SimdS
    {
        public int Vn { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdRegS(inst, address, opCode, false);
        public new static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdRegS(inst, address, opCode, true);

        public OpCode32SimdRegS(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode, isThumb)
        {
            bool single = Size != 3;
            if (single)
            {
                Vn = ((opCode >> 7) & 0x1) | ((opCode >> 15) & 0x1e);
            }
            else
            {
                Vn = ((opCode >> 3) & 0x10) | ((opCode >> 16) & 0xf);
            }
        }
    }
}

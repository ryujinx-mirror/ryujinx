namespace ARMeilleure.Decoders
{
    class OpCode32SimdRegS : OpCode32SimdS
    {
        public int Vn { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdRegS(inst, address, opCode);

        public OpCode32SimdRegS(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
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

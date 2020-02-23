namespace ARMeilleure.Decoders
{
    class OpCode32SimdMemImm : OpCode32, IOpCode32Simd
    {
        public int Vd { get; private set; }
        public int Rn { get; private set; }
        public int Size { get; private set; }
        public bool Add { get; private set; }
        public int Immediate { get; private set; }

        public OpCode32SimdMemImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Immediate = opCode & 0xff;

            Rn = (opCode >> 16) & 0xf;
            Size = (opCode >> 8) & 0x3;

            Immediate <<= (Size == 1) ? 1 : 2;

            bool u = (opCode & (1 << 23)) != 0;
            Add = u;

            bool single = Size != 3;

            if (single)
            {
                Vd = ((opCode >> 22) & 0x1) | ((opCode >> 11) & 0x1e);
            }
            else
            {
                Vd = ((opCode >> 18) & 0x10) | ((opCode >> 12) & 0xf);
            }
        }
    }
}

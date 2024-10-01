namespace ARMeilleure.Decoders
{
    class OpCode32SimdCvtTB : OpCode32, IOpCode32Simd
    {
        public int Vd { get; }
        public int Vm { get; }
        public bool Op { get; } // Convert to Half / Convert from Half
        public bool T { get; } // Top / Bottom
        public int Size { get; } // Double / Single

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdCvtTB(inst, address, opCode, false);
        public static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdCvtTB(inst, address, opCode, true);

        public OpCode32SimdCvtTB(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode)
        {
            IsThumb = isThumb;

            Op = ((opCode >> 16) & 0x1) != 0;
            T = ((opCode >> 7) & 0x1) != 0;
            Size = ((opCode >> 8) & 0x1);

            RegisterSize = Size == 1 ? RegisterSize.Int64 : RegisterSize.Int32;

            if (Size == 1)
            {
                if (Op)
                {
                    Vm = ((opCode >> 1) & 0x10) | ((opCode >> 0) & 0xf);
                    Vd = ((opCode >> 22) & 0x1) | ((opCode >> 11) & 0x1e);
                }
                else
                {
                    Vm = ((opCode >> 5) & 0x1) | ((opCode << 1) & 0x1e);
                    Vd = ((opCode >> 18) & 0x10) | ((opCode >> 12) & 0xf);
                }
            }
            else
            {
                Vm = ((opCode >> 5) & 0x1) | ((opCode << 1) & 0x1e);
                Vd = ((opCode >> 22) & 0x1) | ((opCode >> 11) & 0x1e);
            }
        }
    }
}

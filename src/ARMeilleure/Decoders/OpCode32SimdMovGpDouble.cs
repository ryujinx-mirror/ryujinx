namespace ARMeilleure.Decoders
{
    class OpCode32SimdMovGpDouble : OpCode32, IOpCode32Simd
    {
        public int Size => 3;

        public int Vm { get; }
        public int Rt { get; }
        public int Rt2 { get; }
        public int Op { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdMovGpDouble(inst, address, opCode, false);
        public static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdMovGpDouble(inst, address, opCode, true);

        public OpCode32SimdMovGpDouble(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode)
        {
            IsThumb = isThumb;

            // Which one is used is instruction dependant.
            Op = (opCode >> 20) & 0x1;

            Rt = (opCode >> 12) & 0xf;
            Rt2 = (opCode >> 16) & 0xf;

            bool single = (opCode & (1 << 8)) == 0;
            if (single)
            {
                Vm = ((opCode >> 5) & 0x1) | ((opCode << 1) & 0x1e);
            }
            else
            {
                Vm = ((opCode >> 1) & 0x10) | ((opCode >> 0) & 0xf);
            }
        }
    }
}

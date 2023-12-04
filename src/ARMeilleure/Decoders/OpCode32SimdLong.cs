namespace ARMeilleure.Decoders
{
    class OpCode32SimdLong : OpCode32SimdBase
    {
        public bool U { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdLong(inst, address, opCode, false);
        public static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdLong(inst, address, opCode, true);

        public OpCode32SimdLong(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode, isThumb)
        {
            int imm3h = (opCode >> 19) & 0x7;

            // The value must be a power of 2, otherwise it is the encoding of another instruction.
            switch (imm3h)
            {
                case 1:
                    Size = 0;
                    break;
                case 2:
                    Size = 1;
                    break;
                case 4:
                    Size = 2;
                    break;
            }

            U = ((opCode >> (isThumb ? 28 : 24)) & 0x1) != 0;

            RegisterSize = RegisterSize.Simd64;

            Vd = ((opCode >> 18) & 0x10) | ((opCode >> 12) & 0xf);
            Vm = ((opCode >> 1) & 0x10) | ((opCode >> 0) & 0xf);
        }
    }
}

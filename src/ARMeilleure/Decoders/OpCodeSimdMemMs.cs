namespace ARMeilleure.Decoders
{
    class OpCodeSimdMemMs : OpCodeMemReg, IOpCodeSimd
    {
        public int Reps { get; }
        public int SElems { get; }
        public int Elems { get; }
        public bool WBack { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeSimdMemMs(inst, address, opCode);

        public OpCodeSimdMemMs(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            switch ((opCode >> 12) & 0xf)
            {
                case 0b0000:
                    Reps = 1;
                    SElems = 4;
                    break;
                case 0b0010:
                    Reps = 4;
                    SElems = 1;
                    break;
                case 0b0100:
                    Reps = 1;
                    SElems = 3;
                    break;
                case 0b0110:
                    Reps = 3;
                    SElems = 1;
                    break;
                case 0b0111:
                    Reps = 1;
                    SElems = 1;
                    break;
                case 0b1000:
                    Reps = 1;
                    SElems = 2;
                    break;
                case 0b1010:
                    Reps = 2;
                    SElems = 1;
                    break;

                default:
                    Instruction = InstDescriptor.Undefined;
                    return;
            }

            Size = (opCode >> 10) & 3;
            WBack = ((opCode >> 23) & 1) != 0;

            bool q = ((opCode >> 30) & 1) != 0;

            if (!q && Size == 3 && SElems != 1)
            {
                Instruction = InstDescriptor.Undefined;

                return;
            }

            Extend64 = false;

            RegisterSize = q
                ? RegisterSize.Simd128
                : RegisterSize.Simd64;

            Elems = (GetBitsCount() >> 3) >> Size;
        }
    }
}

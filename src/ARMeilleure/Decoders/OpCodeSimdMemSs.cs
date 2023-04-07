namespace ARMeilleure.Decoders
{
    class OpCodeSimdMemSs : OpCodeMemReg, IOpCodeSimd
    {
        public int  SElems    { get; }
        public int  Index     { get; }
        public bool Replicate { get; }
        public bool WBack     { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeSimdMemSs(inst, address, opCode);

        public OpCodeSimdMemSs(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            int size   = (opCode >> 10) & 3;
            int s      = (opCode >> 12) & 1;
            int sElems = (opCode >> 12) & 2;
            int scale  = (opCode >> 14) & 3;
            int l      = (opCode >> 22) & 1;
            int q      = (opCode >> 30) & 1;

            sElems |= (opCode >> 21) & 1;

            sElems++;

            int index = (q << 3) | (s << 2) | size;

            switch (scale)
            {
                case 1:
                {
                    if ((size & 1) != 0)
                    {
                        Instruction = InstDescriptor.Undefined;

                        return;
                    }

                    index >>= 1;

                    break;
                }

                case 2:
                {
                    if ((size & 2) != 0 ||
                       ((size & 1) != 0 && s != 0))
                    {
                        Instruction = InstDescriptor.Undefined;

                        return;
                    }

                    if ((size & 1) != 0)
                    {
                        index >>= 3;

                        scale = 3;
                    }
                    else
                    {
                        index >>= 2;
                    }

                    break;
                }

                case 3:
                {
                    if (l == 0 || s != 0)
                    {
                        Instruction = InstDescriptor.Undefined;

                        return;
                    }

                    scale = size;

                    Replicate = true;

                    break;
                }
            }

            Index  = index;
            SElems = sElems;
            Size   = scale;

            Extend64 = false;

            WBack = ((opCode >> 23) & 1) != 0;

            RegisterSize = q != 0
                ? RegisterSize.Simd128
                : RegisterSize.Simd64;
        }
    }
}
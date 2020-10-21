namespace ARMeilleure.Decoders
{
    class OpCodeSimdRegElem : OpCodeSimdReg
    {
        public int Index { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeSimdRegElem(inst, address, opCode);

        public OpCodeSimdRegElem(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            switch (Size)
            {
                case 1:
                    Index = (opCode >> 20) & 3 |
                            (opCode >>  9) & 4;

                    Rm &= 0xf;

                    break;

                case 2:
                    Index = (opCode >> 21) & 1 |
                            (opCode >> 10) & 2;

                    break;

                default: Instruction = InstDescriptor.Undefined; break;
            }
        }
    }
}
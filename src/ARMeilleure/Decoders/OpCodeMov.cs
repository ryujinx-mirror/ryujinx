namespace ARMeilleure.Decoders
{
    class OpCodeMov : OpCode
    {
        public int Rd { get; }

        public long Immediate { get; }

        public int Bit { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeMov(inst, address, opCode);

        public OpCodeMov(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            int p1 = (opCode >> 22) & 1;
            int sf = (opCode >> 31) & 1;

            if (sf == 0 && p1 != 0)
            {
                Instruction = InstDescriptor.Undefined;

                return;
            }

            Rd = (opCode >> 0) & 0x1f;
            Immediate = (opCode >> 5) & 0xffff;
            Bit = (opCode >> 21) & 0x3;

            Bit <<= 4;

            Immediate <<= Bit;

            RegisterSize = (opCode >> 31) != 0
                ? RegisterSize.Int64
                : RegisterSize.Int32;
        }
    }
}

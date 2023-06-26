namespace ARMeilleure.Decoders
{
    class OpCodeMemLit : OpCode, IOpCodeLit
    {
        public int Rt { get; }
        public long Immediate { get; }
        public int Size { get; }
        public bool Signed { get; }
        public bool Prefetch { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeMemLit(inst, address, opCode);

        public OpCodeMemLit(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rt = opCode & 0x1f;

            Immediate = (long)address + DecoderHelper.DecodeImmS19_2(opCode);

            switch ((opCode >> 30) & 3)
            {
                case 0:
                    Size = 2;
                    Signed = false;
                    Prefetch = false;
                    break;
                case 1:
                    Size = 3;
                    Signed = false;
                    Prefetch = false;
                    break;
                case 2:
                    Size = 2;
                    Signed = true;
                    Prefetch = false;
                    break;
                case 3:
                    Size = 0;
                    Signed = false;
                    Prefetch = true;
                    break;
            }
        }
    }
}

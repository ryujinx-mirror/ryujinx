namespace ARMeilleure.Decoders
{
    class OpCodeT16BImmCmp : OpCodeT16, IOpCode32BImm
    {
        public int Rn { get; }

        public long Immediate { get; }

        public static new OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT16BImmCmp(inst, address, opCode);

        public OpCodeT16BImmCmp(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rn = (opCode >> 0) & 0x7;

            int imm = ((opCode >> 2) & 0x3e) | ((opCode >> 3) & 0x40);
            Immediate = (int)GetPc() + imm;
        }
    }
}

namespace ARMeilleure.Decoders
{
    class OpCodeSimdTbl : OpCodeSimdReg
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeSimdTbl(inst, address, opCode);

        public OpCodeSimdTbl(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Size = ((opCode >> 13) & 3) + 1;
        }
    }
}

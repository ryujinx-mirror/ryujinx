namespace ARMeilleure.Decoders
{
    class OpCodeSimdMemPair : OpCodeMemPair, IOpCodeSimd
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeSimdMemPair(inst, address, opCode);

        public OpCodeSimdMemPair(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Size = ((opCode >> 30) & 3) + 2;

            Extend64 = false;

            DecodeImm(opCode);
        }
    }
}

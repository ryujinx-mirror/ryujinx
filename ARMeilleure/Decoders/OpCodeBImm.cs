namespace ARMeilleure.Decoders
{
    class OpCodeBImm : OpCode, IOpCodeBImm
    {
        public long Immediate { get; protected set; }

        public OpCodeBImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode) { }
    }
}
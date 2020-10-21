namespace ARMeilleure.Decoders
{
    class OpCodeBImm : OpCode, IOpCodeBImm
    {
        public long Immediate { get; protected set; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeBImm(inst, address, opCode);

        public OpCodeBImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode) { }
    }
}
namespace ARMeilleure.Decoders
{
    class OpCodeCcmpImm : OpCodeCcmp, IOpCodeAluImm
    {
        public long Immediate => RmImm;

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeCcmpImm(inst, address, opCode);

        public OpCodeCcmpImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode) { }
    }
}
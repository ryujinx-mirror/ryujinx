namespace ARMeilleure.Decoders
{
    class OpCodeCcmpImm : OpCodeCcmp, IOpCodeAluImm
    {
        public long Immediate => RmImm;

        public OpCodeCcmpImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode) { }
    }
}
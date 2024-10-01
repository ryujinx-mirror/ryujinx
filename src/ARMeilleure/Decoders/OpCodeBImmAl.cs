namespace ARMeilleure.Decoders
{
    class OpCodeBImmAl : OpCodeBImm
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeBImmAl(inst, address, opCode);

        public OpCodeBImmAl(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Immediate = (long)address + DecoderHelper.DecodeImm26_2(opCode);
        }
    }
}

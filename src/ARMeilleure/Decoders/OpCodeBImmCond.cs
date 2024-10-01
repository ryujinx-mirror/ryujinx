namespace ARMeilleure.Decoders
{
    class OpCodeBImmCond : OpCodeBImm, IOpCodeCond
    {
        public Condition Cond { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeBImmCond(inst, address, opCode);

        public OpCodeBImmCond(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            int o0 = (opCode >> 4) & 1;

            if (o0 != 0)
            {
                Instruction = InstDescriptor.Undefined;

                return;
            }

            Cond = (Condition)(opCode & 0xf);

            Immediate = (long)address + DecoderHelper.DecodeImmS19_2(opCode);
        }
    }
}

namespace ARMeilleure.Decoders
{
    class OpCode32BImm : OpCode32, IOpCode32BImm
    {
        public long Immediate { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32BImm(inst, address, opCode);

        public OpCode32BImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            uint pc = GetPc();

            // When the condition is never, the instruction is BLX to Thumb mode.
            if (Cond != Condition.Nv)
            {
                pc &= ~3u;
            }

            Immediate = pc + DecoderHelper.DecodeImm24_2(opCode);

            if (Cond == Condition.Nv)
            {
                long H = (opCode >> 23) & 2;

                Immediate |= H;
            }
        }
    }
}
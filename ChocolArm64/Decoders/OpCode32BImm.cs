using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCode32BImm : OpCode32, IOpCode32BImm
    {
        public long Imm { get; private set; }

        public OpCode32BImm(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            uint pc = GetPc();

            // When the condition is never, the instruction is BLX to Thumb mode.
            if (Cond != Condition.Nv)
            {
                pc &= ~3u;
            }

            Imm = pc + DecoderHelper.DecodeImm24_2(opCode);

            if (Cond == Condition.Nv)
            {
                long H = (opCode >> 23) & 2;

                Imm |= H;
            }
        }
    }
}
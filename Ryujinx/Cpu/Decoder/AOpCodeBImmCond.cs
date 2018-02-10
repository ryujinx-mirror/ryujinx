using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeBImmCond : AOpCodeBImm, IAOpCodeCond
    {
        public ACond Cond { get; private set; }

        public AOpCodeBImmCond(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            int O0 = (OpCode >> 4) & 1;

            if (O0 != 0)
            {
                Emitter = AInstEmit.Und;

                return;
            }

            Cond = (ACond)(OpCode & 0xf);

            Imm = Position + ADecoderHelper.DecodeImmS19_2(OpCode);
        }
    }
}
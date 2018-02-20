using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeBImmAl : AOpCodeBImm
    {
        public AOpCodeBImmAl(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Imm = Position + ADecoderHelper.DecodeImm26_2(OpCode);
        }
    }
}
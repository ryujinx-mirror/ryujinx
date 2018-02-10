using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeBImmTest : AOpCodeBImm
    {
        public int Rt  { get; private set; }
        public int Pos { get; private set; }

        public AOpCodeBImmTest(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Rt = OpCode & 0x1f;

            Imm = Position + ADecoderHelper.DecodeImmS14_2(OpCode);

            Pos  = (OpCode >> 19) & 0x1f;
            Pos |= (OpCode >> 26) & 0x20;
        }
    }
}
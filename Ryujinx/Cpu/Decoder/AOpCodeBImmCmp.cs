using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeBImmCmp : AOpCodeBImm
    {
        public int Rt { get; private set; }

        public AOpCodeBImmCmp(AInst Inst, long Position, int OpCode) : base(Inst, Position)
        {
            Rt = OpCode & 0x1f;

            Imm = Position + ADecoderHelper.DecodeImmS19_2(OpCode);
        }
    }
}
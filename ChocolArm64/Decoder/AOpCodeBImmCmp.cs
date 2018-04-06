using ChocolArm64.Instruction;
using ChocolArm64.State;

namespace ChocolArm64.Decoder
{
    class AOpCodeBImmCmp : AOpCodeBImm
    {
        public int Rt { get; private set; }

        public AOpCodeBImmCmp(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Rt = OpCode & 0x1f;

            Imm = Position + ADecoderHelper.DecodeImmS19_2(OpCode);

            RegisterSize = (OpCode >> 31) != 0
                ? ARegisterSize.Int64
                : ARegisterSize.Int32;
        }
    }
}
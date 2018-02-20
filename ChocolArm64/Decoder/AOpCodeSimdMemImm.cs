using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdMemImm : AOpCodeMemImm, IAOpCodeSimd
    {
        public AOpCodeSimdMemImm(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Size |= (OpCode >> 21) & 4;

            if (!WBack && !Unscaled && Size >= 4)
            {
                Imm <<= 4;
            }

            Extend64 = false;
        }
    }
}
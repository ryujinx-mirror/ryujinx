using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdMemReg : AOpCodeMemReg, IAOpCodeSimd
    {
        public AOpCodeSimdMemReg(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Size |= (OpCode >> 21) & 4;

            Extend64 = false;
        }
    }
}
using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdTbl : AOpCodeSimdReg
    {
        public AOpCodeSimdTbl(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Size = ((OpCode >> 13) & 3) + 1;
        }
    }
}
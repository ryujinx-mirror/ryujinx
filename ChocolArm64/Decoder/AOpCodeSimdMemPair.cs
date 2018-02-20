using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdMemPair : AOpCodeMemPair, IAOpCodeSimd
    {
        public AOpCodeSimdMemPair(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Size = ((OpCode >> 30) & 3) + 2;

            Extend64 = false;

            DecodeImm(OpCode);
        }
    }
}
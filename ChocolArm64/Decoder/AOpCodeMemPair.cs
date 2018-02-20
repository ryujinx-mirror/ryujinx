using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeMemPair : AOpCodeMemImm
    {
        public int Rt2 { get; private set; }

        public AOpCodeMemPair(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Rt2      =  (OpCode >> 10) & 0x1f;
            WBack    = ((OpCode >> 23) & 0x1) != 0;
            PostIdx  = ((OpCode >> 23) & 0x3) == 1;
            Extend64 = ((OpCode >> 30) & 0x3) == 1;
            Size     = ((OpCode >> 31) & 0x1) | 2;

            DecodeImm(OpCode);
        }

        protected void DecodeImm(int OpCode)
        {
            Imm = ((long)(OpCode >> 15) << 57) >> (57 - Size);
        }
    }
}
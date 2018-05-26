using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder32
{
    class A32OpCodeBImmAl : A32OpCode
    {
        public int Imm;
        public int H;

        public A32OpCodeBImmAl(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Imm = (OpCode <<  8) >> 6;
            H   = (OpCode >> 23) &  2;
        }
    }
}
using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeMemReg : AOpCodeMem
    {
        public bool Shift { get; private set; }
        public int  Rm    { get; private set; }

        public AIntType IntType { get; private set; }

        public AOpCodeMemReg(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Shift    =           ((OpCode >> 12) & 0x1) != 0;
            IntType  = (AIntType)((OpCode >> 13) & 0x7);
            Rm       =            (OpCode >> 16) & 0x1f;
            Extend64 =           ((OpCode >> 22) & 0x3) == 2;
        }
    }
}
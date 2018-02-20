using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdFcond : AOpCodeSimdReg, IAOpCodeCond
    {
        public int NZCV { get; private set; }

        public ACond Cond { get; private set; }

        public AOpCodeSimdFcond(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            NZCV =         (OpCode >>  0) & 0xf;
            Cond = (ACond)((OpCode >> 12) & 0xf);
        }
    }
}
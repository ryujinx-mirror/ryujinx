using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdExt : AOpCodeSimdReg
    {
        public int Imm4 { get; private set; }

        public AOpCodeSimdExt(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            int Imm4 = (OpCode >> 11) & 0xf;
        }
    }
}
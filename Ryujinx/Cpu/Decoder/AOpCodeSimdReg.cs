using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdReg : AOpCodeSimd
    {
        public int  Rm   { get; private set; }
        public bool Bit3 { get; private set; }

        public AOpCodeSimdReg(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Rm   =  (OpCode >> 16) & 0x1f;
            Bit3 = ((OpCode >>  3) & 0x1) != 0;
        }
    }
}
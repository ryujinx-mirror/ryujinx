using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdReg : AOpCodeSimd
    {
        public bool Bit3 { get; private   set; }
        public int  Ra   { get; private   set; }
        public int  Rm   { get; protected set; }

        public AOpCodeSimdReg(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Bit3 = ((OpCode >>  3) & 0x1) != 0;
            Ra   =  (OpCode >> 10) & 0x1f;
            Rm   =  (OpCode >> 16) & 0x1f;
        }
    }
}
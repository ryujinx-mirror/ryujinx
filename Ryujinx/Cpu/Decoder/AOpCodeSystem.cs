using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSystem : AOpCode
    {
        public int Rt  { get; private set; }
        public int Op2 { get; private set; }
        public int CRm { get; private set; }
        public int CRn { get; private set; }
        public int Op1 { get; private set; }
        public int Op0 { get; private set; }

        public AOpCodeSystem(AInst Inst, long Position, int OpCode) : base(Inst, Position)
        {
            Rt  =  (OpCode >>  0) & 0x1f;
            Op2 =  (OpCode >>  5) & 0x7;
            CRm =  (OpCode >>  8) & 0xf;
            CRn =  (OpCode >> 12) & 0xf;
            Op1 =  (OpCode >> 16) & 0x7;
            Op0 = ((OpCode >> 19) & 0x1) | 2;
        }
    }
}
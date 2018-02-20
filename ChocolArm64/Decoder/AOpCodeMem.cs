using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeMem : AOpCode
    {
        public int  Rt       { get; protected set; }
        public int  Rn       { get; protected set; }
        public int  Size     { get; protected set; }
        public bool Extend64 { get; protected set; }

        public AOpCodeMem(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Rt   = (OpCode >>  0) & 0x1f;
            Rn   = (OpCode >>  5) & 0x1f;
            Size = (OpCode >> 30) & 0x3;
        }
    }
}
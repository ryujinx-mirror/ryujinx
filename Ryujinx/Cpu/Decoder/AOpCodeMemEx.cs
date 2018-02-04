using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeMemEx : AOpCodeMem
    {
        public int Rt2 { get; private set; }
        public int Rs  { get; private set; }

        public AOpCodeMemEx(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Rt2 = (OpCode >> 10) & 0x1f;
            Rs  = (OpCode >> 16) & 0x1f;
        }
    }
}
using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeMul : AOpCodeAlu
    {
        public int Rm { get; private set; }
        public int Ra { get; private set; }

        public AOpCodeMul(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Ra = (OpCode >> 10) & 0x1f;
            Rm = (OpCode >> 16) & 0x1f;
        }
    }
}
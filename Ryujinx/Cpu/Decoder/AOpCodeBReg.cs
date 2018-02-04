using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeBReg : AOpCode
    {
        public int Rn { get; private set; }

        public AOpCodeBReg(AInst Inst, long Position, int OpCode) : base(Inst, Position)
        {
            int Op4 = (OpCode >>  0) & 0x1f;
            int Op2 = (OpCode >> 16) & 0x1f;

            if (Op2 != 0b11111 || Op4 != 0b00000)
            {
                Emitter = AInstEmit.Und;

                return;
            }

            Rn = (OpCode >> 5) & 0x1f;
        }
    }
}
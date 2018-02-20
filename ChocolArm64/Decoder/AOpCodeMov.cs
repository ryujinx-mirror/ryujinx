using ChocolArm64.Instruction;
using ChocolArm64.State;

namespace ChocolArm64.Decoder
{
    class AOpCodeMov : AOpCode
    {
        public int  Rd  { get; private set; }
        public long Imm { get; private set; }
        public int  Pos { get; private set; }

        public AOpCodeMov(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            int P1 = (OpCode >> 22) & 1;
            int SF = (OpCode >> 31) & 1;

            if (SF == 0 && P1 != 0)
            {
                Emitter = AInstEmit.Und;

                return;
            }

            Rd  = (OpCode >>  0) & 0x1f;
            Imm = (OpCode >>  5) & 0xffff;
            Pos = (OpCode >> 21) & 0x3;

            Pos <<= 4;
            Imm <<= Pos;

            RegisterSize = (OpCode >> 31) != 0
                ? ARegisterSize.Int64
                : ARegisterSize.Int32;
        }
    }
}
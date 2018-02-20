using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeCcmpImm : AOpCodeCcmp, IAOpCodeAluImm
    {
        public long Imm => RmImm;

        public AOpCodeCcmpImm(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode) { }
    }
}
using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeBImm : AOpCode
    {
        public long Imm { get; protected set; }

        public AOpCodeBImm(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode) { }
    }
}
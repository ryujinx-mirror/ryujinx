using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeException : AOpCode
    {
        public int Id { get; private set; }

        public AOpCodeException(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Id = (OpCode >> 5) & 0xffff;
        }
    }
}
using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeCsel : AOpCodeAlu, IAOpCodeCond
    {
        public int Rm { get; private set; }

        public ACond Cond { get; private set; }

        public AOpCodeCsel(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Rm   =         (OpCode >> 16) & 0x1f;
            Cond = (ACond)((OpCode >> 12) & 0xf);
        }
    }
}
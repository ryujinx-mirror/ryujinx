using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeAluRx : AOpCodeAlu, IAOpCodeAluRx
    {
        public int Shift { get; private set; }
        public int Rm    { get; private set; }

        public AIntType IntType { get; private set; }

        public AOpCodeAluRx(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Shift   =            (OpCode >> 10) & 0x7;
            IntType = (AIntType)((OpCode >> 13) & 0x7);
            Rm      =            (OpCode >> 16) & 0x1f;
        }
    }
}
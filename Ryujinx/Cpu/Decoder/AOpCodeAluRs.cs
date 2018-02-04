using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeAluRs : AOpCodeAlu, IAOpCodeAluRs
    {
        public int Shift { get; private set; }
        public int Rm    { get; private set; }

        public AShiftType ShiftType { get; private set; }

        public AOpCodeAluRs(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Shift     =              (OpCode >> 10) & 0x3f;
            Rm        =              (OpCode >> 16) & 0x1f;
            ShiftType = (AShiftType)((OpCode >> 22) & 0x3);

            //Assert ShiftType != 3
        }
    }
}
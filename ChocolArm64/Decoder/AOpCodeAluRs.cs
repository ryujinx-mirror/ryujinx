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
            int Shift = (OpCode >> 10) & 0x3f;

            if (Shift >= GetBitsCount())
            {
                Emitter = AInstEmit.Und;

                return;
            }

            this.Shift = Shift;

            Rm        =              (OpCode >> 16) & 0x1f;
            ShiftType = (AShiftType)((OpCode >> 22) & 0x3);
        }
    }
}
using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeCcmpReg : AOpCodeCcmp, IAOpCodeAluRs
    {
        public int Rm => RmImm;

        public int Shift => 0;

        public AShiftType ShiftType => AShiftType.Lsl;

        public AOpCodeCcmpReg(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode) { }
    }
}
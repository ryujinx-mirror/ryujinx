using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeCcmpReg64 : OpCodeCcmp64, IOpCodeAluRs64
    {
        public int Rm => RmImm;

        public int Shift => 0;

        public ShiftType ShiftType => ShiftType.Lsl;

        public OpCodeCcmpReg64(Inst inst, long position, int opCode) : base(inst, position, opCode) { }
    }
}